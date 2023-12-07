using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions.Http;
using System.Buffers;
using System.IO.Compression;
using System.Text;


namespace MiniWebServer.MiniApp.Content
{
    public class CompressedStreamContent : MiniContent
    {
        private const int BufferSize = 1024 * 8;

        private static readonly byte[] CRLF_Bytes = Encoding.ASCII.GetBytes("\r\n");
        private static readonly byte[] EndOfChunked_CRLF_Bytes = Encoding.ASCII.GetBytes("0\r\n\r\n");

        private readonly Stream stream;
        private readonly int compressionQuality;
        private readonly bool autoCloseStream;
        private readonly HttpHeaders headers;
        private readonly ILogger<CompressedStreamContent> logger;

        public CompressedStreamContent(Stream stream, IMiniAppRequestContext? context = null, int compressionQuality = 5, bool autoCloseStream = true)
        {
            if (compressionQuality < 0 || compressionQuality > 11)
            {
                throw new ArgumentOutOfRangeException(nameof(compressionQuality), "compressionQuality must be from 0 (no compression) to 11 (max compression)");
            }

            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
            {
                throw new IOException("stream is not readable");
            }

            if (context != null)
            {
                var loggerFactory = context.Services.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    logger = loggerFactory.CreateLogger<CompressedStreamContent>();
                }
            }
            logger ??= NullLogger<CompressedStreamContent>.Instance;

            this.compressionQuality = compressionQuality;
            this.autoCloseStream = autoCloseStream;

            headers = new()
            {
                { "Transfer-Encoding", "chunked" },
                { "Content-Encoding", "br" },
            };
        }

        public override HttpHeaders Headers => headers;


        public override async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                // we will use chunked transfer-encoding for all files, but I'm thinking about using a cache for small files (check CompressionCaching.ICompressionCache)

                long originalSize = 0;
                long compressedSize = 0;

                var encoder = new BrotliEncoder(quality: compressionQuality, window: 24);

                var bytesRead = await this.stream.ReadAsync(buffer, cancellationToken);

                while (bytesRead > 0)
                {
                    originalSize += bytesRead;

                    Write(ref encoder, buffer[..bytesRead], stream, ref compressedSize, false);

                    bytesRead = await this.stream.ReadAsync(buffer, cancellationToken);
                }

                Write(ref encoder, Array.Empty<byte>(), stream, ref compressedSize, true);

                encoder.Dispose();

                stream.Write(EndOfChunked_CRLF_Bytes);

                compressedSize += EndOfChunked_CRLF_Bytes.Length;

                logger.LogDebug("Compressed {t} bytes to {c} bytes", originalSize, compressedSize);

                return compressedSize;
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
                    this.stream.Close();
                }
            }
        }

        private static void Write(ref BrotliEncoder encoder, byte[] data, Stream stream, ref long lBytesWritten, bool isFinalBlock)
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
                    var lengthBytes = Encoding.ASCII.GetBytes(bytesWritten.ToString("X"));

                    stream.Write(lengthBytes);
                    stream.Write(CRLF_Bytes);
                    stream.Write(compressBuffer.AsSpan(0, bytesWritten));
                    stream.Write(CRLF_Bytes);

                    lBytesWritten += bytesWritten + lengthBytes.Length + 4; // total of bytes sent including 2 CRLFs 
                }

                if (bytesConsumed > 0) buffer = buffer[bytesConsumed..];
            }

            ArrayPool<byte>.Shared.Return(compressBuffer);
        }
    }
}
