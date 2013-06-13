using System;
using System.Web;
using System.Net.Mime;

namespace Imazen.SlimResponse
{
    public class SlimResponseModule : System.Web.IHttpModule
    {
        public const string moduleKey = "SlimResponseModule.Installed";

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
            var context = sender != null ? ((HttpApplication)sender).Context : null;
                
            if (context != null && !context.Items.Contains(moduleKey))
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


