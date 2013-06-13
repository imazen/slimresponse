using System;
using System.IO;
using System.Web;

namespace Imazen.SlimResponse
{
    /// <summary>
    /// A semi-generic Stream implementation for Response.Filter with an event interface for handling Content transformations via Stream or String.
    /// </summary>
    /// <remarks>
    /// Use with care for large output as this implementation copies the output into a memory stream and so increases memory usage.
    /// http://www.west-wind.com/weblog/posts/2009/Nov/13/Capturing-and-Transforming-ASPNET-Output-with-ResponseFilter
    /// </remarks>
    public class ResponseFilterStream : Stream
    {
        /// <summary>
        /// The original stream
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// Current position in the original stream
        /// </summary>
        private long _position;

        /// <summary>
        /// Stream that original content is read into and then passed to TransformStream function
        /// </summary>
        private MemoryStream _cacheStream = new MemoryStream(5000);

        /// <summary>
        /// Internal pointer that that keeps track of the size of the cacheStream
        /// </summary>
        private int _cachePointer = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseFilterStream"/> class.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        public ResponseFilterStream(Stream responseStream)
        {
            this._stream = responseStream;
        }

        /// <summary>
        /// Determines whether the stream is captured
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is captured; otherwise, <c>false</c>.
        /// </value>
        private bool IsCaptured
        {
            get
            {
                if (this.CaptureStream != null || this.CaptureString != null || this.TransformStream != null || this.TransformString != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether the Write method is outputting data immediately or delaying output until Flush() is fired.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is output delayed; otherwise, <c>false</c>.
        /// </value>
        private bool IsOutputDelayed
        {
            get
            {
                if (this.TransformStream != null || this.TransformString != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Event that captures Response output and makes it available as a MemoryStream instance.
        /// Output is captured but won't affect Response output.
        /// </summary>
        public event Action<MemoryStream> CaptureStream;

        /// <summary>
        /// Event that captures Response output and makes it available as a string.
        /// Output is captured but won't affect Response output.
        /// </summary>
        public event Action<string> CaptureString;

        /// <summary>
        /// Event that allows you transform the stream as each chunk of the output is written in the Write() operation of the stream.
        /// This means that that it's possible/likely that the input buffer will not contain the full response output but only one of potentially many chunks.
        /// 
        /// This event is called as part of the filter stream's Write() operation.
        /// </summary>
        public event Func<byte[], byte[]> TransformWrite;

        /// <summary>
        /// Event that allows you to transform the response stream as each chunk of bytep[] output is written during the stream's write operation.
        /// This means it's possibly/likely that the string passed to the handler only contains a portion of the full output.
        /// Typical buffer chunks are around 16k a piece.
        /// 
        /// This event is called as part of the stream's Write operation.
        /// </summary>
        public event Func<string, string> TransformWriteString;

        /// <summary>
        /// This event allows capturing and transformation of the entire output stream by caching all write operations and delaying final response output until Flush() is called on the stream.
        /// </summary>
        public event Func<MemoryStream, MemoryStream> TransformStream;

        /// <summary>
        /// Event that can be hooked up to handle Response.Filter Transformation. Passed a string that you can modify and return back as a return value.
        /// The modified content will become the final output.
        /// </summary>
        public event Func<string, string> TransformString;

        /// <summary>
        /// Called when [capture stream].
        /// </summary>
        /// <param name="ms">The ms.</param>
        protected virtual void OnCaptureStream(MemoryStream ms)
        {
            if (this.CaptureStream != null)
            {
                this.CaptureStream(ms);
            }
        }

        /// <summary>
        /// Called when [capture string internal].
        /// </summary>
        /// <param name="ms">The ms.</param>
        private void OnCaptureStringInternal(MemoryStream ms)
        {
            if (this.CaptureString != null)
            {
                string content = HttpContext.Current.Response.ContentEncoding.GetString(ms.ToArray());
                this.OnCaptureString(content);
            }
        }

        /// <summary>
        /// Called when [capture string].
        /// </summary>
        /// <param name="output">The output.</param>
        protected virtual void OnCaptureString(string output)
        {
            if (this.CaptureString != null)
            {
                this.CaptureString(output);
            }
        }

        /// <summary>
        /// Called when [transform write].
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        protected virtual byte[] OnTransformWrite(byte[] buffer)
        {
            if (this.TransformWrite != null)
            {
                return this.TransformWrite(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// Called when [transform write string internal].
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        private byte[] OnTransformWriteStringInternal(byte[] buffer)
        {
            var encoding = HttpContext.Current.Response.ContentEncoding;
            var output = this.OnTransformWriteString(encoding.GetString(buffer));
            return encoding.GetBytes(output);
        }

        /// <summary>
        /// Called when [transform write string].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string OnTransformWriteString(string value)
        {
            if (this.TransformWriteString != null)
            {
                return this.TransformWriteString(value);
            }

            return value;
        }

        /// <summary>
        /// Called when [transform complete stream].
        /// </summary>
        /// <param name="ms">The ms.</param>
        /// <returns></returns>
        protected virtual MemoryStream OnTransformCompleteStream(MemoryStream ms)
        {
            if (this.TransformStream != null)
            {
                return this.TransformStream(ms);
            }

            return ms;
        }

        /// <summary>
        /// Allows transforming of strings
        /// Note this handler is internal and not meant to be overridden as the TransformString Event has to be hooked up in order for this handler to even fire to avoid the overhead of string conversion on every pass through.
        /// </summary>
        /// <param name="responseText">The response text.</param>
        /// <returns></returns>
        private string OnTransformCompleteString(string responseText)
        {
            if (this.TransformString != null)
            {
                this.TransformString(responseText);
            }

            return responseText;
        }

        /// <summary>
        /// Wrapper method form OnTransformString that handles
        /// stream to string and vice versa conversions
        /// </summary>
        /// <param name="ms">The ms.</param>
        /// <returns></returns>
        internal MemoryStream OnTransformCompleteStringInternal(MemoryStream ms)
        {
            if (this.TransformString == null)
            {
                return ms;
            }

            string content = HttpContext.Current.Response.ContentEncoding.GetString(ms.ToArray());

            content = this.TransformString(content);
            byte[] buffer = HttpContext.Current.Response.ContentEncoding.GetBytes(content);
            ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);

            return ms;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get
            {
                return this._position;
            }

            set
            {
                this._position = value;
            }
        }

        /// <summary>
        /// Seeks the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin direction)
        {
            return this._stream.Seek(offset, direction);
        }

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name="length">The length.</param>
        public override void SetLength(long length)
        {
            this._stream.SetLength(length);
        }

        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {
            this._stream.Close();
        }

        /// <summary>
        /// Override flush by writing out the cached stream data
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            if (this.IsCaptured && this._cacheStream.Length > 0)
            {
                // Check for transform implementations
                this._cacheStream = this.OnTransformCompleteStream(this._cacheStream);
                this._cacheStream = this.OnTransformCompleteStringInternal(this._cacheStream);

                this.OnCaptureStream(this._cacheStream);
                this.OnCaptureStringInternal(this._cacheStream);

                // write the stream back out if output was delayed
                if (this.IsOutputDelayed)
                {
                    this._stream.Write(this._cacheStream.ToArray(), 0, (int)this._cacheStream.Length);
                }

                // Clear the cache once we've written it out
                this._cacheStream.SetLength(0);
            }

            // default flush behavior
            this._stream.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this._stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Overriden to capture output written by ASP.NET and captured
        /// into a cached stream that is written out later when Flush()
        /// is called.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.IsCaptured)
            {
                // copy to holding buffer only - we'll write out later
                this._cacheStream.Write(buffer, 0, count);
                this._cachePointer += count;
            }

            // just transform this buffer
            if (this.TransformWrite != null)
            {
                buffer = this.OnTransformWrite(buffer);
            }

            if (this.TransformWriteString != null)
            {
                buffer = this.OnTransformWriteStringInternal(buffer);
            }

            if (!this.IsOutputDelayed)
            {
                this._stream.Write(buffer, offset, buffer.Length);
            }
        }
    }
    }
