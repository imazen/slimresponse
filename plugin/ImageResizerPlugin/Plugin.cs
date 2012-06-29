using System;
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;

using ImageResizer;
using ImageResizer.Configuration;
using ImageResizer.Configuration.Xml;
using ImageResizer.Configuration.Issues;
using ImageResizer.Plugins;

namespace ResponsivePresets
{
    public partial class ImageResizerPlugin : ImageResizer.Plugins.IPlugin
    {
        // - all defined preset prefixes
        Dictionary<string, PresetPrefix> prefixes = new Dictionary<string, PresetPrefix>(StringComparer.OrdinalIgnoreCase);

        // fetch the config to be available globally here
        Config c;

        public IPlugin Install(ImageResizer.Configuration.Config c)
        {
            // - register plugin
            this.c = c;
            c.Plugins.add_plugin(this);
            c.Pipeline.PostAuthorizeRequestStart += Pipeline_PostAuthorizeRequestStart;
            ParseXml(c.getConfigXml().queryFirst("responsivepresets"), c);

            return this;
        }

        public bool Uninstall(ImageResizer.Configuration.Config c)
        {
            c.Plugins.remove_plugin(this);
            c.Pipeline.PostAuthorizeRequestStart -= Pipeline_PostAuthorizeRequestStart;
            return true;
        }

        void Pipeline_PostAuthorizeRequestStart(System.Web.IHttpModule sender, System.Web.HttpContext context)
        {
            // - if we have no cookie and the preset name starts with TWO dots, we deliver the SINGLE dot variant!
            // - this is to confuse the browser cache for the javsascript enabled clients as they will get the responsive variant in less than a second ...
            if ((context.Request.Cookies["responsive-width"] == null || string.IsNullOrEmpty(context.Request.Cookies["responsive-width"].Value)) && c.Pipeline.ModifiedQueryString["preset"] != null && c.Pipeline.ModifiedQueryString["preset"].StartsWith(".."))
            {
                // - rewrite preset name (remove ONE of the TWO dots)
                c.Pipeline.ModifiedQueryString["preset"] = c.Pipeline.ModifiedQueryString["preset"].Substring(1);

                // - just let it pass on through the request and imageresizer pipeline
                return;
            }

            // - if we have a cookie, intercept requests with presets that start with  a dot "."
            if (c.Pipeline.ModifiedQueryString["preset"] != null && c.Pipeline.ModifiedQueryString["preset"].StartsWith(".") && context.Request.Cookies["responsive-width"] != null)
            {
                try
                {
                    string matchingPrefix = "";

                    // - client screen width
                    int clientScreenWidth = int.Parse(context.Request.Cookies["responsive-width"].Value);

                    // find matching prefix
                    foreach (string p in prefixes.Keys)
                    {
                        if (clientScreenWidth >= prefixes[p].MinWidth && (clientScreenWidth <= prefixes[p].MaxWidth || prefixes[p].MaxWidth == 0))
                        {
                            matchingPrefix = p;
                            break;
                        }
                    }

                    // - append responsive preset prefix
                    c.Pipeline.ModifiedQueryString["preset"] = matchingPrefix + c.Pipeline.ModifiedQueryString["preset"];
                }
                catch
                {
                    // - ignore for now to not break requests
                }
            }
        }

        protected void ParseXml(Node n, Config conf)
        {
            if (n == null) return;

            if (n.Children == null) return;
            foreach (Node c in n.Children)
            {
                string name = c.Attrs["prefix"];
                if (c.Name.Equals("preset", StringComparison.OrdinalIgnoreCase))
                {
                    // - verify the name is specified and unique
                    if (string.IsNullOrEmpty(name) || prefixes.ContainsKey(name))
                    {
                        conf.configurationSectionIssues.AcceptIssue(
                            new Issue("ResponsivePresets",
                            "The prefix attribute for each preset must be specified, and it must be unique.",
                            "XML: " + c.ToString(), IssueSeverity.ConfigurationError)
                        );
                        continue;
                    }

                    prefixes[name] = new PresetPrefix(int.Parse(c.Attrs["minwidth"]), int.Parse(c.Attrs["maxwidth"]));
                }
            }
            return;
        }

    }
}