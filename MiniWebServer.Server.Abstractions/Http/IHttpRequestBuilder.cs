using MiniWebServer.Abstractions.Http;
using System.IO.Pipelines;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.Abstractions.Http
{
    public interface IHttpRequestBuilder
    {
        HttpRequest Build();

        IHttpRequestBuilder SetMethod(HttpMethod method);
        IHttpRequestBuilder SetUrl(string url);
        IHttpRequestBuilder AddCookie(HttpCookie cookie);
        IHttpRequestBuilder AddCookie(HttpCookies cookie);
        IHttpRequestBuilder AddHeader(string name, string value);
        IHttpRequestBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> headers);
        IHttpRequestBuilder SetHeaders(HttpRequestHeaders requestHeaders);
        IHttpRequestBuilder AddTransferEncoding(string transferEncodings);
        IHttpRequestBuilder SetParameters(HttpParameters parameters);
        IHttpRequestBuilder SetQueryString(string queryString);
        IHttpRequestBuilder SetHash(string hash);
        IHttpRequestBuilder SetBodyPipeline(Pipe bodyPipeline);
        IHttpRequestBuilder SetContentLength(long contentLength);
        IHttpRequestBuilder SetContentType(string contentType);
        IHttpRequestBuilder SetSegments(string[] segments);
        IHttpRequestBuilder SetHttps(bool isHttps);
        IHttpRequestBuilder SetPort(int port);
        IHttpRequestBuilder SetHost(string host);

        HttpRequestHeaders RequestHeaders { get; }
    }
}
