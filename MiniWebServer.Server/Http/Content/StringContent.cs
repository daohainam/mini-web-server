using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Content
{
    public class StringContent : Abstractions.Http.HttpContent
    {
        private readonly string content;

        public StringContent(string content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override async Task WriteTo(StreamWriter writer)
        {
            await writer.WriteAsync(content);
        }
    }
}
