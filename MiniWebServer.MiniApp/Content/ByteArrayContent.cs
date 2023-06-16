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
    public class ByteArrayContent : MiniContent
    {
        private readonly byte[] content;
        private readonly HttpHeaders headers;

        public ByteArrayContent(byte[] content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            headers = new();
        }

        public override long ContentLength => content.LongLength;

        public override HttpHeaders Headers => headers;

        public override async Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken)
        {
            writer.Write(content);

            return await Task.FromResult(content.Length);
        }
    }
}
