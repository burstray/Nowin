// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LiteDB;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// This middleware provides an efficient fallback mechanism for sending static files
    /// when the server does not natively support such a feature.
    /// The caller is responsible for setting all headers in advance.
    /// The caller is responsible for performing the correct impersonation to give access to the file.
    /// </summary>
    public class SendFileMiddleware
    {
        private readonly AppFunc _next;

        /// <summary>
        /// Creates a new instance of the SendFileMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public SendFileMiddleware(AppFunc next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            _next = next;
        }

        /// <summary>
        /// Adds the sendfile.SendAsync Func to the request environment, if not already present.
        /// </summary>
        /// <param name="environment">OWIN environment dictionary which stores state information about the request, response and relevant server state.</param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);

            // Check if there is a SendFile delegate already presents
            if (context.Get<object>(Constants.SendFileAsyncKey) as SendFileFunc == null)
            {
                context.Set<SendFileFunc>(Constants.SendFileAsyncKey, new SendFileFunc(new SendFileWrapper(context.Response.Body).SendAsync));
            }

            return _next(environment);
        }

        private class SendFileWrapper
        {
            private readonly Stream _output;

            internal SendFileWrapper(Stream output)
            {
                _output = output;
            }

            // Not safe for overlapped writes.
            internal Task SendAsync(string fileName, long offset, long? length, CancellationToken cancel)
            {
                cancel.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentNullException("fileName");
                }
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(string.Empty, fileName);
                }

                var fileInfo = new FileInfo(fileName);
                if (offset < 0 || offset > fileInfo.Length)
                {
                    throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
                }

                if (length.HasValue &&
                    (length.Value < 0 || length.Value > fileInfo.Length - offset))
                {
                    throw new ArgumentOutOfRangeException("length", length, string.Empty);
                }

                if (fileName.ToLower().EndsWith(".mp4"))
                {
                    ////MemoryStream ms = new MemoryStream(new byte[1024 * 64], true);

                    //// Open database (or create if doesn't exist)
                    //using (var db = new LiteDatabase(@"I:\!GitHub\Nowin\OwinHostingSample\bin\Debug\output\LiteDB.db"))
                    //{
                    //    // Gets a FileStorage with the default collections
                    //    var fs = db.FileStorage;

                    //    //// Gets a FileStorage with custom collection name
                    //    //var fs = db.GetStorage<string>("myFiles", "myChunks");

                    //    // Upload a file from file system
                    //    //fs.Upload("test.mp4", @"I:\!GitHub\Nowin\OwinHostingSample\bin\Debug\output\videos\IMG_4865.MP4");

                    //    //// Upload a file from a Stream
                    //    //fs.Upload("$/photos/2014/picture-01.jpg", "picture-01.jpg", stream);

                    //    // Find file reference only - returns null if not found
                    //    var file = fs.FindById("test.mp4");

                    //    //// Now, load binary data and save to file system
                    //    //file.SaveAs(@"C:\Temp\new-picture.jpg");

                    //    // Or get binary data as Stream and copy to another Stream

                    //    var litedbstream = file.OpenRead();

                    //    //file.CopyTo(ms);
                    //    //fileStream = file.OpenRead();

                    //    //    fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 64,
                    //    //FileOptions.Asynchronous | FileOptions.SequentialScan);

                    //    //// Find all files references in a "directory"
                    //    //var files = fs.Find("$/photos/2014/");

                    Stream litedbstream = null;
                    if (fileName.ToLower().EndsWith(".mp4"))
                    {
                        FileStreamFactory fsf = new FileStreamFactory("G:\\TestVR\\liteaes.db", "123456", true, false);
                        litedbstream = fsf.GetStream(false, true);
                        //ab.Seek(4096 + 32, SeekOrigin.Begin);
                        //byte[] byt = new byte[ab.Length];
                        //ab.Read(byt, 0, byt.Length);
                    }

                    try
                    {
                        litedbstream.Seek(offset, SeekOrigin.Begin);//此操作后Stream的数据自然就变了
                        var copyOperation = new StreamCopyOperation(litedbstream, _output, length, cancel);
                        return copyOperation.Start()
                            .ContinueWith(resultTask =>
                            {
                                litedbstream.Close();
                                resultTask.Wait(); // Throw exceptions, etc.
                            }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception)
                    {
                        litedbstream.Close();
                        throw;
                    }
                    //}
                }
                else
                {
                    Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 64, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    try
                    {
                        fileStream.Seek(offset, SeekOrigin.Begin);
                        var copyOperation = new StreamCopyOperation(fileStream, _output, length, cancel);
                        return copyOperation.Start()
                            .ContinueWith(resultTask =>
                            {
                                fileStream.Close();
                                resultTask.Wait(); // Throw exceptions, etc.
                            }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception)
                    {
                        fileStream.Close();
                        throw;
                    }
                }
            }
        }
    }
}
