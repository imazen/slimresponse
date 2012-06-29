using System;
using System.Web;
using System.Net.Mime;

namespace ResponsivePresets.FilterModule
{
    public class RegisterFilterModule : System.Web.IHttpModule
    {
        public const string moduleKey = "ResponsivePresetsRegisterFilterModule.Installed";

        public void Dispose()
        {
            // nothing, nix, zero, nada
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {

            context.ReleaseRequestState += new EventHandler(this.RegisterCookielessImageRequestFilter);
            context.PreSendRequestHeaders += new EventHandler(this.RegisterCookielessImageRequestFilter);
        }

        private void RegisterCookielessImageRequestFilter(object sender, EventArgs e)
        {
            // - only kick in if the cookie isn't set
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["responsive-width"] == null)
            {
                var context = HttpContext.Current;
                
                if (!context.Items.Contains(moduleKey))
                {
                    var response = context.Response;
                    var currentExecutionFilePath = context.Request.CurrentExecutionFilePath;

                    if (response.ContentType == MediaTypeNames.Text.Html)
                    {
                        var parser = new Parser(response.ContentEncoding);
                        var filter = new ResponseFilterStream(response.Filter);
                        filter.TransformString += new Func<string, string>(parser.RewriteImageUrls);
                        response.Filter = filter;
                    }

                    context.Items.Add(moduleKey, new object());
                }
            }
        }


    }
}


