using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Web;

[assembly: OwinStartup(typeof(OwinHostingSample.Startup))]

namespace OwinHostingSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = x =>
            //    {
            //        if (x.OwinContext.Request.Path.Value.ToLower().EndsWith(".mp4"))
            //        {
            //            System.Windows.Forms.MessageBox.Show(x.OwinContext.Request.Path.Value);
            //            using (var sr = File.OpenRead(x.File.PhysicalPath))
            //            {
            //                //Context hr = x.OwinContext.Response;
            //                x.OwinContext.Response.ContentLength = sr.Length;
            //                sr.CopyTo(x.OwinContext.Response.Body);
            //            }
            //        }
            //    }
            //});

            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.Run(context =>
            {
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Hello , this is console appliaction from self owin.");
            });
        }
    }
}
