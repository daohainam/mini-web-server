using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MvcMiddlewareTests;

internal class RequestHeadersContainer : IRequestHeadersContainer
{
    public required HttpRequestHeaders Headers { get; init; }
}
