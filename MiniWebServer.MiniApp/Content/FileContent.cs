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
    public class FileContent : MiniContent
    {
        private readonly FileInfo file;
        private readonly HttpHeaders headers;
        private readonly FileContentRange? fileContentRange;

        public FileContent(string fileName, FileContentRange? fileContentRange = null) : this(new FileInfo(fileName), fileContentRange)
        {
        }
        public FileContent(FileInfo file, FileContentRange? fileContentRange = null)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            if (!file.Exists)
            {
                throw new FileNotFoundException(file.FullName);
            }

            this.fileContentRange = fileContentRange;

            if (fileContentRange != null)
            {
                long length = fileContentRange.LastBytePosInclusive.HasValue ? (fileContentRange.LastBytePosInclusive.Value - fileContentRange.FirstBytePosInclusive + 1) : file.Length - fileContentRange.FirstBytePosInclusive;
                if (length > file.Length - fileContentRange.FirstBytePosInclusive)
                {
                    length = file.Length - fileContentRange.FirstBytePosInclusive;
                }
                headers = new() {
                    { "Content-Length", length.ToString() }
                };
            }
            else
            {
                headers = new() {
                    { "Content-Length", file.Length.ToString() }
                };
            }
        }

        public override HttpHeaders Headers => headers;

        public override async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            long length = file.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(8192); // rent a 8K-buffer

            try
            {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (fileContentRange != null)
                {
                    if (fileContentRange.FirstBytePosInclusive > 0)
                    {
                        fs.Seek(fileContentRange.FirstBytePosInclusive, SeekOrigin.Begin);

                        length = file.Length - fileContentRange.FirstBytePosInclusive;
                    }

                    if (fileContentRange.LastBytePosInclusive.HasValue)
                    {
                        length = fileContentRange.LastBytePosInclusive.Value - fileContentRange.FirstBytePosInclusive + 1; // the the byte positions are inclusive, for example: 0-0 means 1 byte (at [0])

                        if (length > file.Length - fileContentRange.FirstBytePosInclusive) // if the selected representation is shorter than the specified FirstBytePosInclusive - length, the entire representation is used.
                        {
                            length = file.Length - fileContentRange.FirstBytePosInclusive;
                        }
                    }
                }

                var bytesRead = await fs.ReadAsync(buffer, 0, (int)Math.Min(length, buffer.Length), cancellationToken);

                while (length > 0)
                {
                    if (bytesRead != buffer.Length)
                        stream.Write(buffer.AsSpan(0, bytesRead));
                    else
                        stream.Write(buffer.AsSpan());

                    length -= bytesRead;
                    if (length > 0)
                    {
                        bytesRead = await fs.ReadAsync(buffer, cancellationToken);
                    }
                }

                fs.Close();
                return file.Length;
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
