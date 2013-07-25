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

        public string TransformImgToSlimmage(string content)
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
                        // - if no width set with querystring param, but preset name, get the width from the preset
                        if ((src.Value.IndexOf("width=", StringComparison.InvariantCultureIgnoreCase) == -1)
                            && (src.Value.IndexOf("preset=", StringComparison.InvariantCultureIgnoreCase) > -1))
                        {
                            // - get presets from config
                            var configSection = WebConfigurationManager.GetSection("resizer") as ImageResizer.ResizerSection;
                            XmlElement conf = configSection.getCopyOfNode("presets").ToXmlElement();
                            // - find preset by name
                            XmlNode curpreset = conf.SelectSingleNode(string.Format("preset[@name='{0}']/@defaults", System.Web.HttpUtility.ParseQueryString(src.Value.Substring(src.Value.IndexOf("?")))["preset"]));
                            string presetWidth = System.Web.HttpUtility.ParseQueryString(curpreset.InnerText)["width"];

                            // - append presets's default width to querystring
                            src.Value = string.Format("{0}&width={1}", src.Value, presetWidth);
                        }
                        
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
                Trace.TraceWarning("SlimResponse failed to parse HTML: {0} {1}", ex.ToString(), ex.StackTrace);
                // - better that nothing(tm) ...
                //return content;
                return ex.ToString();
            }
        }

    }
}

