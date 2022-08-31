// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.FileSystems;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    // FYI: In most cases the source will be a FileStream and the destination will be to the network.
    internal class StreamCopyOperation
    {
        private const int DefaultBufferSize = 1024 * 4;//_buffer size

        private readonly TaskCompletionSource<object> _tcs;
        private readonly Stream _source;
        private readonly Stream _destination;
        private byte[] _buffer;
        private byte[] _mp4buffer;
        private readonly AsyncCallback _readCallback;
        private readonly AsyncCallback _writeCallback;
        public string _filePath;

        private long? _bytesRemaining;
        private CancellationToken _cancel;

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, CancellationToken cancel, string filePath)
            : this(source, destination, bytesRemaining, DefaultBufferSize, cancel, filePath)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, int bufferSize, CancellationToken cancel, string filePath)
            : this(source, destination, bytesRemaining, new byte[bufferSize], cancel, filePath)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, byte[] buffer, CancellationToken cancel, string filePath)
        {
            Contract.Assert(source != null);
            Contract.Assert(destination != null);
            Contract.Assert(!bytesRemaining.HasValue || bytesRemaining.Value >= 0);
            Contract.Assert(buffer != null);

            _source = source;
            _destination = destination;
            _bytesRemaining = bytesRemaining;
            _cancel = cancel;
            _buffer = buffer;
            _filePath = filePath;

            _tcs = new TaskCompletionSource<object>();
            _readCallback = new AsyncCallback(ReadCallback);
            _writeCallback = new AsyncCallback(WriteCallback);
        }

        internal Task Start()
        {
            ReadNextSegment();
            return _tcs.Task;
        }

        private void Complete()
        {
            _tcs.TrySetResult(null);
        }

        private bool CheckCancelled()
        {
            if (_cancel.IsCancellationRequested)
            {
                _tcs.TrySetCanceled();
                return true;
            }
            return false;
        }

        private void Fail(Exception ex)
        {
            _tcs.TrySetException(ex);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadNextSegment()
        {
            // The natural end of the range.
            if (_bytesRemaining.HasValue && _bytesRemaining.Value <= 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                if (_filePath.ToString().ToLower().EndsWith(".mp4"))//targetFilePath != null && targetFilePath.ToLower().EndsWith(".mp4") && 
                {
                    int readLength = 4096 + 16;
                    _mp4buffer = new byte[4096 + 16];
                    //Console.WriteLine("StreamCopyOperation found mp4 ==>" + targetFilePath);
                    ////physicalPath = _fileInfo.PhysicalPath.Replace(".mp4", "1.mp4");
                    //byte[] _newbuffer = DecryptData(_buffer, "1234560000000000");//20220830 added

                    //int newreadLength = _newbuffer.Length;
                    //if (_bytesRemaining.HasValue)
                    //{
                    //    newreadLength = (int)Math.Min(_bytesRemaining.Value, (long)newreadLength);
                    //}
                    //IAsyncResult newasync = _source.BeginRead(_newbuffer, 0, newreadLength, _readCallback, null);

                    //if (newasync.CompletedSynchronously)
                    //{
                    //    int newread = _source.EndRead(newasync);

                    //    WriteToOutputStream(newread);
                    //}

                    if (_bytesRemaining.HasValue)
                    {
                        //readLength = (int)Math.Min(_bytesRemaining.Value, (long)readLength + 16);//剩余长度和读取长度选择少的那个
                        readLength = (int)Math.Min(_bytesRemaining.Value, (long)readLength);//剩余长度和读取长度选择少的那个
                    }
                    //_mp4buffer = new byte[readLength];//原来的buffer还需要调整长度，那就新建一个吧

                    IAsyncResult async = _source.BeginRead(_mp4buffer, 0, readLength, _readCallback, null);

                    if (async.CompletedSynchronously)
                    {
                        int read = _source.EndRead(async);

                        WriteToOutputStream(read);
                    }
                }
                else
                {
                    int readLength = 4096;
                    _buffer = new byte[4096];

                    if (_bytesRemaining.HasValue)
                    {
                        readLength = (int)Math.Min(_bytesRemaining.Value, (long)readLength);//剩余长度和读取长度选择少的那个
                    }
                    IAsyncResult async = _source.BeginRead(_buffer, 0, readLength, _readCallback, null);

                    if (async.CompletedSynchronously)
                    {
                        int read = _source.EndRead(async);

                        WriteToOutputStream(read);
                    }
                }

            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }

            try
            {
                int read = _source.EndRead(async);
                WriteToOutputStream(read);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void WriteToOutputStream(int count)
        {
            if (_bytesRemaining.HasValue)
            {
                _bytesRemaining -= count;
            }

            // End of the source stream.
            if (count == 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                if (_filePath.ToLower().EndsWith(".mp4"))//targetFilePath != null && targetFilePath.ToLower().EndsWith(".mp4")
                {
                    Console.WriteLine("StreamCopyOperation found mp4 ==>" + _filePath);

                    if (_mp4buffer == null || _mp4buffer.Length == 0)
                    {
                        return;
                    }

                    //physicalPath = _fileInfo.PhysicalPath.Replace(".mp4", "1.mp4");

                    byte[] _bufferToWriteStream = new byte[4096];

                    _bufferToWriteStream = SecurityUtils.AesDecrypt("1234560000000000", _mp4buffer);//Rayx 20220831

                    if (_bufferToWriteStream == null || _bufferToWriteStream.Length == 0)
                    {
                        return;
                    }

                    //int newreadLength = _newbuffer.Length;
                    //if (_bytesRemaining.HasValue)
                    //{
                    //    newreadLength = (int)Math.Min(_bytesRemaining.Value, (long)newreadLength);
                    //}
                    //IAsyncResult newasync = _source.BeginRead(_newbuffer, 0, newreadLength, _readCallback, null);

                    //if (newasync.CompletedSynchronously)
                    //{
                    //    int newread = _source.EndRead(newasync);

                    //    WriteToOutputStream(newread);
                    //}

                    IAsyncResult newasync = _destination.BeginWrite(_bufferToWriteStream, 0, (count >= 16) ? count - 16 : count, _writeCallback, null);//这里的Count需要减下
                    if (newasync.CompletedSynchronously)
                    {
                        _destination.EndWrite(newasync);
                        ReadNextSegment();
                    }
                }
                //if (_buffer == null || _buffer.Length == 0)
                //{
                //    return;
                //}
                else
                {
                    IAsyncResult async = _destination.BeginWrite(_buffer, 0, count, _writeCallback, null);
                    if (async.CompletedSynchronously)
                    {
                        _destination.EndWrite(async);
                        ReadNextSegment();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteToOutputStream exception " + ex.Message);
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void WriteCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }

            try
            {
                _destination.EndWrite(async);
                ReadNextSegment();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }
    }
}
