using System;
using System.Web;
using System.Net.Mime;

namespace Imazen.SlimResponse.FilterModule
{
    public class ActivateFilterModule : System.Web.IHttpModule
    {
        public const string moduleKey = "ResponsivePresetsFilterModule.Installed";

        public void Dispose()
        {
            // nothing, nix, zero, nada
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {

            context.ReleaseRequestState += new EventHandler(this.RegisterMarkupFilter);
            context.PreSendRequestHeaders += new EventHandler(this.RegisterMarkupFilter);
        }

        private void RegisterMarkupFilter(object sender, EventArgs e)
        {
            // - only kick in if not already done so
            var context = HttpContext.Current;
                
            if (!context.Items.Contains(moduleKey))
            {
                var response = context.Response;
                var currentExecutionFilePath = context.Request.CurrentExecutionFilePath;

                if (response.ContentType == MediaTypeNames.Text.Html)
                {
                    var parser = new MarkupParser(response.ContentEncoding);
                    var filter = new ResponseFilterStream(response.Filter);
                    filter.TransformString += new Func<string, string>(parser.TransformImgToPicture);
                    response.Filter = filter;
                }

                context.Items.Add(moduleKey, new object());
            }
        }

    }
}


