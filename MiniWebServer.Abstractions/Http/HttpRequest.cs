using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpRequest : IWebRequest
    {
        public HttpRequest(HttpMethod method, string url, IReadOnlyDictionary<string, string> headers)
        {
            Method = method;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Headers = new Dictionary<string, string>(headers ?? throw new ArgumentNullException(nameof(headers)));
        }

        public HttpMethod Method { get; }

        public string Url { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }
        public string Host { 
            get 
            {
                return TryGetHeader("Host");
            } 
        }

        private string TryGetHeader(string hostName)
        {
            if (Headers.TryGetValue(hostName, out var value))
                return value;

            return string.Empty;
        }
    }
}
