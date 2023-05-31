using MiniWebServer.Helpers;
using MiniWebServer.MiniApp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form
{
    public class XWwwFormUrlencodedFormReader : IFormReader
    {
        private readonly long contentLength;

        public XWwwFormUrlencodedFormReader(long contentLength)
        {
            this.contentLength = contentLength;
        }

        public async Task<IRequestForm?> ReadAsync(PipeReader pipeReader, CancellationToken cancellationToken = default)
        {
            StringBuilder stringBuilder = new();
            long bytesRead = 0;

            ReadResult readResult = await pipeReader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            while (bytesRead < contentLength)
            {
                long maxBytesToRead = Math.Min(contentLength, buffer.Length);

                stringBuilder.Append(Encoding.ASCII.GetString(buffer.Slice(0, maxBytesToRead)));

                bytesRead += maxBytesToRead;
                pipeReader.AdvanceTo(buffer.GetPosition(maxBytesToRead));

                if (bytesRead < contentLength)
                {
                    readResult = await pipeReader.ReadAsync(cancellationToken);
                    buffer = readResult.Buffer;
                }
            }

            var form = new RequestForm();

            // now we have read the content, it's time to decode
            string[] strings = UrlHelpers.UrlDecode(stringBuilder.ToString()).Split(new char[] { '&' });
            foreach (string s in strings)
            {
                int idx = s.IndexOf('=');
                if (idx < 0)
                {
                    continue; // we accept some minor errors
                }
                else if (idx == 0)
                {
                    continue; // we accept some minor errors
                }
                else
                {
                    form[s[..idx]] = s[(idx + 1)..];
                }
            }

            return form;
        }
    }
}
