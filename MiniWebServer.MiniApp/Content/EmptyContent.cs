using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class EmptyContent : MiniContent
    {
        private readonly HttpHeaders headers;

        public EmptyContent()
        {
            headers = new();
        }

        public override long ContentLength => 0;

        public override HttpHeaders Headers => headers;

        public override async Task<int> WriteToAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken)
        {
            return await Task.FromResult(0);
        }

        public static EmptyContent Instance => new();
    }
}
