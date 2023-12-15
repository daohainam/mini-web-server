using MiniWebServer.Abstractions.Http.Header;

namespace MiniWebServer.Abstractions.Http
{
    // here is the this of standard request headers defined in 
    // TODO: using TryGetValueAsString is not a good practice, we should have a better/faster way to store headers (cache to separate fields)
    public class HttpRequestHeaders : HttpHeaders
    {
        public HttpRequestHeaders()
        {
        }

        public HttpRequestHeaders(string name, string value) : this()
        {
            Add(name, value);
        }

        public HttpRequestHeaders(params HttpHeader[] headers) : this()
        {
            foreach (var header in headers)
            {
                Add(header);
            }
        }

        public string CacheControl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public HostHeader? Host { get; set; } = null;
        public string Origin { get; set; } = string.Empty;
        public string TransferEncoding { get; set; } = string.Empty;
        public string[] AcceptEncoding { get; set; } = [];
        public HttpCookies Cookie { get; set; } = [];
        public string SecWebSocketKey { get; set; } = string.Empty;
        public string Upgrade { get; set; } = string.Empty;
        public string AcceptLanguage { get; set; } = string.Empty;
        public AuthorizationHeader? Authorization { get; set; } = null;
        public string Connection { get; set; } = string.Empty;
        public RangeHeader? Range { get; set; } = null;
        public string SecWebSocketProtocol { get; set; } = string.Empty;
        public string SecWebSocketVersion { get; set; } = string.Empty;
        public long ContentLength { get; set; } = 0;
    }
}
