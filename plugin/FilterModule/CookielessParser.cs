using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ResponsivePresets.FilterModule
{
    public class Parser
    {
        private System.Text.Encoding TextEncoding;

        public Parser(System.Text.Encoding encoding)
        {
            this.TextEncoding = encoding;
        }

        public string RewriteImageUrls(string content)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                foreach (HtmlNode img in doc.DocumentNode.SelectNodes("//img[@src]"))
                {
                    HtmlAttribute src = img.Attributes["src"];
                    if (src != null)
                    {
                        if (src.Value.ToLowerInvariant().IndexOf("preset=.") != -1)
                        {
                            //src.Value = Regex.Replace(src.Value, "preset=.", "preset=..", RegexOptions.IgnoreCase);
                            src.Value = ReplaceString(src.Value, "preset=.", "preset=..", StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }

                return doc.DocumentNode.OuterHtml;
            }
            catch
            {
                // - better that nothing(tm) ...
                return content;
            }
        }

        public string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }
}

