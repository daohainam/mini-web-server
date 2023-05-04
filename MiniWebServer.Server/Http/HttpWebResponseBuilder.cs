using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniWebServer.Server.Http
{
    public class HttpWebResponseBuilder : IHttpResponseBuilder
    {
        private HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        private Dictionary<string, string> headers = new();
        private string reasonPhrase = string.Empty;
        private Abstractions.Http.HttpContent content = new Content.StringContent(string.Empty);

        public HttpResponse Build()
        {
            var response = new HttpResponse(statusCode, reasonPhrase, headers, content);

            return response;
        }

        public IHttpResponseBuilder SetStatusCode(HttpStatusCode httpStatusCode)
        {
            statusCode = httpStatusCode;

            return this;
        }

        public IHttpResponseBuilder SetReasonPhrase(string reasonPhrase)
        {
            this.reasonPhrase = reasonPhrase ?? string.Empty;

            return this;
        }

        public IHttpResponseBuilder SetContent(Abstractions.Http.HttpContent content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));

            return this;
        }

        public IHttpResponseBuilder AddHeader(string name, string value)
        {
            headers.TryAdd(name, value);

            return this;
        }

        public IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            foreach (var k in headers)
            {
                headers.TryAdd(k.Key, k.Value);
            }

            return this;
        }

    }
}
