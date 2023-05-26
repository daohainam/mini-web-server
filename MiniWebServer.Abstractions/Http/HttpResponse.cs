using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponse : IHttpResponse
    {
        public HttpResponse(HttpResponseCodes statusCode, string reasonPhrase, HttpResponseHeaders headers, HttpCookies cookies, IHttpContent content)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase ?? throw new ArgumentNullException(nameof(reasonPhrase));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Cookies = cookies ?? throw new ArgumentNullException(nameof(cookies));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public HttpResponseCodes StatusCode { get; }
        public string ReasonPhrase { get; }
        public HttpResponseHeaders Headers { get; }
        public IHttpContent Content { get; }
        public HttpCookies Cookies { get; }
    }
}
