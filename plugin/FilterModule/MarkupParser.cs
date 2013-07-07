using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using HtmlAgilityPack;
using System.Xml;
using System.Diagnostics;

namespace Imazen.SlimResponse
{
    public class MarkupParser
    {
        private System.Text.Encoding TextEncoding;

        public MarkupParser(System.Text.Encoding encoding)
        {
            this.TextEncoding = encoding;
        }

        public string TransformImgToPicture(string content)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                // - activating"do no harm"-mode
                doc.OptionFixNestedTags = false;
                doc.OptionAutoCloseOnEnd = false;
                doc.OptionCheckSyntax = false;
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//img[@src]");
                if (collection == null)
                    return content;
                var changed = false;
                foreach (HtmlNode img in collection)
                {
                    HtmlAttribute src = img.Attributes["src"];
                    HtmlAttribute cls = img.Attributes["class"];

                    if ((src != null && src.Value.IndexOf("slimmage=true", StringComparison.InvariantCultureIgnoreCase) > -1)
                        || (cls != null && cls.Value.IndexOf("slimmage",  StringComparison.InvariantCultureIgnoreCase) > -1))
                    {
                        
                        // - append fallback image
                        HtmlNode container = doc.CreateElement("noscript");
                        container.SetAttributeValue("data-slimmage", "true");
                        //copy attributes for IE6/7/8 support
                        foreach (var a in img.Attributes)
                        {
                            container.SetAttributeValue("data-img-" + a.Name, a.Value);
                        }
                        //Place 'img' inside 'noscript'
                        img.ParentNode.InsertBefore(container, img);
                        img.Remove();
                        container.AppendChild(img);
                        changed = true;
                    }
                }
                //Don't modify the DOM unless you actually edited the HTML
                return changed ? doc.DocumentNode.OuterHtml : content;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("SlimResponse failed to parse HTML: " + ex.ToString() + ex.StackTrace);
                // - better that nothing(tm) ...
                //return content;
                return ex.ToString();
            }
        }

    }
}

