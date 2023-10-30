using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMiddlewareTests
{
    internal class RequestBodyReader : IRequestBodyReader
    {
        private readonly string body;

        public RequestBodyReader(string body)
        {
            this.body = body ?? string.Empty;
        }

        public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(body);
        }
    }
}
