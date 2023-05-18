using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
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
        public HttpResponse(HttpStatusCode statusCode, string reasonPhrase, HttpResponseHeaders headers, MiniContent content)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase ?? throw new ArgumentNullException(nameof(reasonPhrase));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public HttpResponseHeaders Headers { get; }
        public MiniContent Content { get; }
    }
}
