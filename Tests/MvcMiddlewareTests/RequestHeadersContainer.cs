using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMiddlewareTests
{
    internal class RequestHeadersContainer: IRequestHeadersContainer
    {
        public required HttpRequestHeaders Headers { get; init; }
    }
}
