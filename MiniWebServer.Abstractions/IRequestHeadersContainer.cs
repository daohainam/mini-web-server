using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IRequestHeadersContainer
    {
        HttpRequestHeaders Headers { get; }
    }
}