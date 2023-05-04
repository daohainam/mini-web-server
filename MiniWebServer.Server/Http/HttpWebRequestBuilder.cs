using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniWebServer.Server.Http
{
    public class HttpWebRequestBuilder : IHttpRequestBuilder
    {
        private HttpMethod httpMethod = HttpMethod.Get;
        private Dictionary<string, string> headers = new();
        private string url = "/";

        public IHttpRequestBuilder AddHeader(string name, string value)
        {
            headers.TryAdd(name, value);

            return this;
        }

        public IHttpRequestBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            foreach (var k in headers)
            {
                headers.TryAdd(k.Key, k.Value);
            }

            return this;
        }

        public HttpRequest Build()
        {
            var request = new HttpRequest(httpMethod, url, headers);

            return request;
        }

        public IHttpRequestBuilder SetMethod(HttpMethod method)
        {
            httpMethod = method;

            return this;
        }
        public IHttpRequestBuilder SetUrl(string url)
        {
            this.url = url;

            return this;
        }
    }
}
