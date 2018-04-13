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
      if (!string.IsNullOrWhiteSpace(firstPart) && firstPart.Contains(">"))
      {
        var parts = firstPart.Split('"');
        var unvedrsionedMediaTemplates = Context.Database.GetItem("{00373F71-5D08-4E9A-840F-9BB3C8193518}").Children;
        var linkIdAttribute = args.FieldValue.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)
          .SingleOrDefault(x => x.StartsWith("id="))?.Replace("id=", "")?.Replace("\"", "");
        ID mediaItemID;
        if (!ID.TryParse(linkIdAttribute, out mediaItemID))
        {
          return;
        }

        var mediaItem = Context.Database.GetItem(mediaItemID);
        foreach (string part in parts)
        {
          if (!MediaManager.IsMediaUrl(part) || unvedrsionedMediaTemplates.Any(x => x.ID == mediaItem.TemplateID))
          {
            continue;
          }

          // If the media url doesn't have an 'la' parameter, add one
          if (!part.Contains("?la=") && !part.Contains("&la="))
          {
            string lang = Context.Language.Name;
            string newUrl = $"{part}{(part.Contains('?') ? "&" : "?")}la={lang}";
            args.Result.FirstPart = args.Result.FirstPart.Replace(part, newUrl);
          }
        }
      }
    }
  }
}