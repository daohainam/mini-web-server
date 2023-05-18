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
    public class HttpRequest : IHttpRequest
    {
        public HttpRequest(HttpMethod method, string url, HttpRequestHeaders headers)
        {
            Method = method;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }

        public HttpMethod Method { get; }

        public string Url { get; }

        public HttpRequestHeaders Headers { get; }
        public bool KeepAliveRequested { get 
            {
                return Headers.Any(h => string.Equals("Connection", h.Value.Name, StringComparison.InvariantCultureIgnoreCase) && h.Value.Value.Any() && string.Equals("Keep-Alive", h.Value.Value.First(), StringComparison.InvariantCultureIgnoreCase));
            } 
        }
    }
}
