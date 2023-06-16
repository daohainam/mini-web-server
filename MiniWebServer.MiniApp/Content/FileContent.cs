using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class FileContent : MiniContent
    {
        private readonly FileInfo file;
        private readonly HttpHeaders headers;

        public FileContent(string fileName): this(new FileInfo(fileName))
        {
        }
        public FileContent(FileInfo file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            if (!file.Exists)
            {
                throw new FileNotFoundException(file.FullName);
            }

            headers = new();
        }

        public override HttpHeaders Headers => headers;

        public override long ContentLength => file.Length;

        public override async Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken)
        {
            if (ContentLength == 0)
                return 0;

            long length = file.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(8192); // rent a 8K-buffer

            try
            {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var bytesRead = await fs.ReadAsync(buffer, cancellationToken);

                while (length > 0)
                {
                    if (bytesRead != buffer.Length)
                        writer.Write(buffer.AsSpan(0, bytesRead));
                    else
                        writer.Write(buffer.AsSpan());

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
