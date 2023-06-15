using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public static class RequestBodyHelpers
    {
        public static async Task<string> ReadAsStringAsync(this IHttpRequest request, CancellationToken cancellationToken = default)
        {
            var contentLength = request.ContentLength;
            if (contentLength > 0)
            {
                var reader = request.BodyManager.GetReader() ?? throw new InvalidOperationException("Body reader cannot be null");
                var encoding = Encoding.UTF8; // todo: we should take the right encoding from Content-Type
                var sb = new StringBuilder();

                ReadResult readResult = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                long bytesRead = 0;
                while (bytesRead < contentLength)
                {
                    long maxBytesToRead = contentLength - bytesRead;
                    if (buffer.Length >= maxBytesToRead)
                    {
                        sb.Append(encoding.GetString(buffer.Slice(0, maxBytesToRead))); // what will happen if a multi-byte character is partly sent?

                        reader.AdvanceTo(buffer.GetPosition(maxBytesToRead));
                        break;
                    }
                    else if (buffer.Length > 0)
                    {
                        sb.Append(encoding.GetString(buffer));
                        reader.AdvanceTo(buffer.GetPosition(buffer.Length));

                        bytesRead += buffer.Length;
                    }

                    readResult = await reader.ReadAsync(cancellationToken);
                    buffer = readResult.Buffer;
                }

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static async Task<T?> ReadAsJsonAsync<T>(this IHttpRequest request, CancellationToken cancellationToken = default)
        {
            var jsonString = await ReadAsStringAsync(request, cancellationToken);

            if (!string.IsNullOrEmpty(jsonString))
            {
                T? result = JsonSerializer.Deserialize<T?>(jsonString);

                return result;
            }
            else
            {
                return default;
            }
        }
    }
}
