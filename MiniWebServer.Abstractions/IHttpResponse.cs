using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IHttpResponse
    {
        HttpResponseCodes StatusCode { get; set; }
        string ReasonPhrase { get; set; }
        HttpResponseHeaders Headers { get; }
        IHttpContent Content { get; set; }
        HttpCookies Cookies { get; }
        Stream Body { get; set; }
    }
}
