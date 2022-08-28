using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;

namespace OwinHostingSample
{
    static class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:8080";
            var root = args.Length > 0 ? args[0] : ".\\Output";
            var fileSystem = new PhysicalFileSystem(root);
            var options = new FileServerOptions();

#if DEBUG
            options.EnableDirectoryBrowsing = true;
#endif

            options.FileSystem = fileSystem;
            options.StaticFileOptions.ContentTypeProvider = new CustomContentTypeProvider();
            options.StaticFileOptions.OnPrepareResponse = context =>
            {
                Console.WriteLine("options.StaticFileOptions.OnPrepareResponse = context =>" + context.OwinContext.Request.Path.Value);

                var headers = context.OwinContext.Response.Headers;
                //var contentType = headers["Content-Type"];

                if (context.File.Name.ToLower().EndsWith(".mp4"))//if (contentType != "video/mp4" && !context.File.Name.ToLower().EndsWith(".mp4"))
                {
                    //var fileNameToTry = context.File.Name.Substring(0, context.File.Name.Length - 3);
                    var mimeTypeProvider = new FileExtensionContentTypeProvider();
                    Console.WriteLine("mp4 found - options.StaticFileOptions.OnPrepareResponse = context =>" + context.OwinContext.Request.Path.Value);

                    //if (mimeTypeProvider.TryGetContentType(fileNameToTry, out var mimeType))
                    //{
                    //headers.Add("Content-Encoding", "gzip");
                    //headers["Content-Type"] = mimeType;
                    //}

                    //using (var sr = File.OpenRead(context.File.PhysicalPath.Replace(".mp4", "1.mp4")))
                    //{
                    //    //Context hr = x.OwinContext.Response;
                    //    context.OwinContext.Response.ContentLength = sr.Length;
                    //    sr.CopyTo(context.OwinContext.Response.Body);
                    //}
                }
            };

            WebApp.Start(url, builder => builder.UseFileServer(options));

            Console.WriteLine("Listening at " + url);
            Console.ReadLine();
        }
    }

    public class CustomContentTypeProvider : FileExtensionContentTypeProvider
    {
        public CustomContentTypeProvider()
        {
            Mappings.Add(".pano", "video/mp4");
        }
    }
}
