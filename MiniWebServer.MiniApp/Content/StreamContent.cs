using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class StreamContent : MiniContent
    {
        private const int BufferSize = 1024 * 8;
        
        private static readonly byte[] CRLF_Bytes = Encoding.ASCII.GetBytes("\r\n");
        private static readonly byte[] EndOfChunked_CRLF_Bytes = Encoding.ASCII.GetBytes("0\r\n\r\n");

        private readonly Stream inputStream;
        private readonly bool autoCloseStream;
        private readonly HttpHeaders headers;

        public StreamContent(Stream stream): this(stream, true)
        {
        }

        public StreamContent(Stream inputStream, bool autoCloseStream)
        {
            this.inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));

            if (!inputStream.CanRead)
            {
                throw new ArgumentException("stream is not readable");
            }

            this.autoCloseStream = autoCloseStream;

            headers = new();
        }

        public override HttpHeaders Headers => headers;

        public override async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            long totalBytesSent = 0L;

            try
            {
                var bytesRead = await inputStream.ReadAsync(buffer, cancellationToken);

                while (bytesRead > 0)
                {
                    totalBytesSent += bytesRead;

                    stream.Write(Encoding.ASCII.GetBytes(bytesRead.ToString("X")));
                    stream.Write(CRLF_Bytes);
                    stream.Write(buffer.AsSpan()[..bytesRead]); // don't use buffer[..bytesRead], it will create a copy of data
                    stream.Write(CRLF_Bytes);

                    bytesRead = await inputStream.ReadAsync(buffer, cancellationToken);
                }

                stream.Write(EndOfChunked_CRLF_Bytes);

                return totalBytesSent;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);

                if (autoCloseStream)
                {
                    inputStream.Close();
                }
            }
        }
    }
}
