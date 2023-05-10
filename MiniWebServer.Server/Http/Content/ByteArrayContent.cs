using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Content
{
    public class ByteArrayContent : Abstractions.Http.HttpContent
    {
        private readonly byte[] content;

        public ByteArrayContent(byte[] content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override async Task WriteToAsync(Stream stream)
        {
            await stream.WriteAsync(content);
        }
    }
}
