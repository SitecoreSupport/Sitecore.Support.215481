using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.RenderField;
using Sitecore.Resources.Media;

namespace Sitecore.Support.Pipelines.RenderField
{
  public class EnsureLanguageInMediaLinks
  {
    public virtual void Process(RenderFieldArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      string firstPart = args.Result.FirstPart;
      if (string.IsNullOrWhiteSpace(firstPart) || !firstPart.Contains(">"))
      {
        return;
      }

      var parts = firstPart.Split('"');
      string[] fieldValues = this.FieldIsRichText(args)
        ? args.FieldValue.Split('"')
        : args.FieldValue.Split(' ');
      var unvedrsionedMediaTemplates = Context.Database.GetItem("{00373F71-5D08-4E9A-840F-9BB3C8193518}").Children;

      for (int i = 0; i < parts.Count(); i++)
      {
        if (!MediaManager.IsMediaUrl(parts[i]))
        {
          continue;
        }

        var linkIdAttribute = this.FieldIsRichText(args)
          ? fieldValues[i].Replace("-/media/", "").Replace(".ashx", "")
          : fieldValues.SingleOrDefault(x => x.StartsWith("id="))?.Replace("id=", "")?.Replace("\"", "");

        Guid mediaItemId;
        if (!Guid.TryParse(linkIdAttribute, out mediaItemId))
        {
          return;
        }

        var mediaItem = Context.Database.GetItem(ID.Parse(mediaItemId));
        if (unvedrsionedMediaTemplates.Any(x => x.ID.Equals(mediaItem.TemplateID)))
        {
          // If media is unversioned and url has a 'la' parameter, remove one
          if (parts[i].Contains("la="))
          {
            var newPart = parts[i].Replace($"?la={Context.Language.Name}", "")
              .Replace($"&la={Context.Language.Name}", "");
            args.Result.FirstPart = args.Result.FirstPart.Replace(parts[i], newPart);
          }
        }
        else
        {
          // If media is versioned and url doesn't have a 'la' parameter, add one
          if (!parts[i].Contains("la="))
          {
            string newUrl = $"{parts[i]}{(parts[i].Contains('?') ? "&" : "?")}la={Context.Language.Name}";
            args.Result.FirstPart = args.Result.FirstPart.Replace(parts[i], newUrl);
          }
        }
      }
    }

    private bool FieldIsRichText(RenderFieldArgs args)
    {
      return args.FieldTypeKey.Equals("rich text");
    }
  }
}