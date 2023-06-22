using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MiniWebServer.MiniApp.Content
{
    public class CompressedFileContent : MiniContent
    {
        private const int BufferSize = 1024 * 8;

        private static readonly byte[] CRLF_Bytes = Encoding.ASCII.GetBytes("\r\n");
        private static readonly byte[] EndOfChunked_CRLF_Bytes = Encoding.ASCII.GetBytes("0\r\n\r\n");

        private readonly FileInfo file;
        private readonly HttpHeaders headers;
        private readonly ILogger<CompressedFileContent> logger;

        public CompressedFileContent(string fileName, IMiniAppContext context) : this(new FileInfo(fileName), context)
        {
        }
        public CompressedFileContent(FileInfo? file, IMiniAppContext context)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            if (!file.Exists)
            {
                throw new FileNotFoundException(file.FullName);
            }

            var loggerFactory = context.Services.GetService<ILoggerFactory>();
            if (loggerFactory == null)
            {
                logger = NullLogger<CompressedFileContent>.Instance;
            }
            else
            {
                logger = loggerFactory.CreateLogger<CompressedFileContent>();
            }

            headers = new()
            {
                { "Transfer-Encoding", "chunked" },
                { "Content-Encoding", "br" },
            };
        }

        public override HttpHeaders Headers => headers;


        public override async Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken)
        {
            long length = file.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

                // we will use chunked transfer-encoding for all files, but I'm thinking about using a cache for small files (check CompressionCaching.ICompressionCache)

                //long originalSize = 0;
                //long compressedSize = 0;

                var encoder = new BrotliEncoder(quality: 5, window: 24);

                var bytesRead = await fs.ReadAsync(buffer, cancellationToken);

                while (length > 0)
                {
                    //originalSize += bytesRead;

                    length -= bytesRead;

                    Write(ref encoder, buffer[..bytesRead], writer, false);

                    if (length > 0)
                    {
                        bytesRead = await fs.ReadAsync(buffer, cancellationToken);
                    }
                }

                Write(ref encoder, Array.Empty<byte>(), writer, true);

                encoder.Dispose();

                //logger.LogDebug("{c} bytes compressed from {o} bytes", compressedSize, originalSize);

                //if (!dataSent) // if we could not send compressed data, then send it as
                //{
                //    length = file.Length;
                //    var bytesRead = await fs.ReadAsync(buffer, cancellationToken);
                //    while (length > 0)
                //    {
                //        writer.Write(Encoding.ASCII.GetBytes(bytesRead.ToString("X"))); // should be in hex format
                //        writer.Write(CRLF_Bytes);
                //        if (bytesRead != buffer.Length)
                //            writer.Write(buffer.AsSpan(0, bytesRead));
                //        else
                //            writer.Write(buffer.AsSpan());
                //        writer.Write(CRLF_Bytes);

                //        length -= bytesRead;
                //        if (length > 0)
                //        {
                //            bytesRead = await fs.ReadAsync(buffer, cancellationToken);
                //        }
                //    }
                //}


                writer.Write(EndOfChunked_CRLF_Bytes);

                fs.Close();
                return file.Length;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void Write(ref BrotliEncoder encoder, byte[] data, IContentWriter writer, bool isFinalBlock)
        {
            byte[] compressBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            OperationStatus lastResult = OperationStatus.DestinationTooSmall;
            ReadOnlySpan<byte> buffer = data;
            while (lastResult == OperationStatus.DestinationTooSmall)
            {
                lastResult = encoder.Compress(buffer, compressBuffer, out int bytesConsumed, out int bytesWritten, isFinalBlock);
                if (lastResult == OperationStatus.InvalidData) throw new InvalidOperationException();

                //logger.LogDebug("{c} bytes compressed from {o} bytes, result: {r}", bytesWritten, bytesConsumed, lastResult);
                if (bytesWritten > 0)
                {
                    writer.Write(Encoding.ASCII.GetBytes(bytesWritten.ToString("X")));
                    writer.Write(CRLF_Bytes);
                    writer.Write(compressBuffer.AsSpan(0, bytesWritten));
                    writer.Write(CRLF_Bytes);
                }

                if (bytesConsumed > 0) buffer = buffer[bytesConsumed..];
            }

            ArrayPool<byte>.Shared.Return(compressBuffer);
        }
    }
}
