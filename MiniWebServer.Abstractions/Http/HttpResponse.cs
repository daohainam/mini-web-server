using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponse : IWebResponse
    {
        public HttpResponse(HttpStatusCode statusCode, string reasonPhrase, IReadOnlyDictionary<string, string> headers, HttpContent content)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase ?? throw new ArgumentNullException(nameof(reasonPhrase));
            Headers = new Dictionary<string, string>(headers ?? throw new ArgumentNullException(nameof(headers)));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public HttpContent Content { get; }
    }
}
