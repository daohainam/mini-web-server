using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Server.Abstractions.Http
{
    public interface IHttpResponseBuilder
    {
        HttpResponse Build();
        IHttpResponseBuilder SetStatusCode(HttpResponseCodes httpStatusCode);
        IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues);
        IHttpResponseBuilder AddHeader(string name, string value);
        IHttpResponseBuilder AddOrUpdateHeader(string name, string value);
        IHttpResponseBuilder AddOrSkipHeader(string name, string value);
        IHttpResponseBuilder SetReasonPhrase(string message);
        IHttpResponseBuilder SetContent(MiniApp.MiniContent content);
        IHttpResponseBuilder AddCookie(HttpCookie cookie);
        IHttpResponseBuilder AddCookie(IEnumerable<HttpCookie> cookies);
        IHttpResponseBuilder SetHeaderContentEncoding(string contentEncoding);
        IHttpResponseBuilder SetHeaderContentLength(long contentLength);
        IHttpResponseBuilder SetHeaderConnection(string connectionStatus);
    }
}
