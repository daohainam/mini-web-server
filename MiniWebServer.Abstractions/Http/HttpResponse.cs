namespace MiniWebServer.Abstractions.Http;

public class HttpResponse : IHttpResponse
{
    private HttpResponseCodes statusCode;

    public HttpResponse(HttpResponseCodes statusCode, Stream stream,
        string? reasonPhrase = null, HttpResponseHeaders? headers = null, HttpCookies? cookies = null, IHttpContent? content = null)
    {
        StatusCode = statusCode;
        Stream = stream;

        ReasonPhrase = reasonPhrase ?? HttpResponseReasonPhrases.GetReasonPhrase(statusCode);
        Headers = headers ?? [];
        Cookies = cookies ?? [];
        Content = content ?? EmptyContent.Instance;
    }

    public HttpResponseCodes StatusCode
    {
        get
        {
            return statusCode;
        }
        set
        {
            statusCode = value;
            ReasonPhrase = HttpResponseReasonPhrases.GetReasonPhrase(value);
        }
    }
    public string ReasonPhrase { get; set; }
    public HttpResponseHeaders Headers { get; }
    public IHttpContent Content { get; set; }
    public HttpCookies Cookies { get; }
    public Stream Stream { get; set; }
}
