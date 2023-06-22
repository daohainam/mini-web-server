using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class StreamContent : MiniContent
    {
        private readonly Stream stream;
        private readonly HttpHeaders headers;

        public StreamContent(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream is not readable");
            }

            headers = new() {
                { "Content-Length", stream.Length.ToString() }
            };
        }

        public override HttpHeaders Headers => headers;

        public override async Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken)
        {
            long length = stream.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(8192); // rent a 8K-buffer

            try
            {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

                while (length > 0)
                {
                    if (bytesRead != buffer.Length)
                        writer.Write(buffer.AsSpan(0, bytesRead));
                    else
                        writer.Write(buffer.AsSpan());

                    length -= bytesRead;
                    if (length > 0)
                    {
                        bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    }
                }

                stream.Close();
                return stream.Length;
            } catch (Exception)
            {
                return 0;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
