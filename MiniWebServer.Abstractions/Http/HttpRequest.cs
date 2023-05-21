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
        public HttpRequest(HttpMethod method, string url, HttpRequestHeaders headers, string queryString, string hash, HttpParameters queryParameters)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ArgumentException($"'{nameof(queryString)}' cannot be null or empty.", nameof(queryString));
            }

            if (string.IsNullOrEmpty(hash))
            {
                throw new ArgumentException($"'{nameof(hash)}' cannot be null or empty.", nameof(hash));
            }

            if (queryParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            Method = method ?? throw new ArgumentNullException(nameof(method));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            QueryString = queryString;
            Hash = hash;
            QueryParameters = queryParameters;
        }

        public HttpMethod Method { get; }

        public string Url { get; }

        public HttpRequestHeaders Headers { get; }
        public string QueryString { get; }
        public string Hash { get; }
        public HttpParameters QueryParameters { get; }

        public bool KeepAliveRequested { get 
            {
                return Headers.Any(h => string.Equals("Connection", h.Value.Name, StringComparison.InvariantCultureIgnoreCase) && h.Value.Value.Any() && string.Equals("Keep-Alive", h.Value.Value.First(), StringComparison.InvariantCultureIgnoreCase));
            } 
        }
    }
}
