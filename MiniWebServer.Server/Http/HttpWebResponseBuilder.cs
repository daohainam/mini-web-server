using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniWebServer.Server.Http
{
    public class HttpWebResponseBuilder : IHttpResponseBuilder, IHttpResponse
    {
        private HttpResponseCodes statusCode = HttpResponseCodes.InternalServerError;
        private readonly HttpResponseHeaders headers = new();
        private string reasonPhrase = string.Empty;
        private readonly HttpCookies cookies = new();
        private MiniContent content = new global::MiniWebServer.MiniApp.Content.StringContent(string.Empty);

        public HttpResponseCodes StatusCode => statusCode;

        public string ReasonPhrase => reasonPhrase;

        public HttpResponseHeaders Headers => headers;

        public IHttpContent Content => content;

        public HttpCookies Cookies => cookies;

        public HttpResponse Build()
        {
            if (!headers.HasName("Content-Length"))
            {
                headers.Add("Content-Length", content.ContentLength.ToString());
            }
            if (!headers.HasName("Content-Type"))
            {
                headers.Add("Content-Type", "text/plain");
            }           
            if (!headers.HasName("Server"))
            {
                headers.Add("Server", "Mini-Web-Server/alpha");
            }

            var response = new HttpResponse(statusCode, reasonPhrase, headers, cookies, content);

            return response;
        }

        public IHttpResponseBuilder SetStatusCode(HttpResponseCodes httpStatusCode)
        {
            statusCode = httpStatusCode;

            return this;
        }

        public IHttpResponseBuilder SetReasonPhrase(string reasonPhrase)
        {
            this.reasonPhrase = reasonPhrase ?? string.Empty;

            return this;
        }

        public IHttpResponseBuilder SetContent(MiniContent content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));

            return this;
        }

        public IHttpResponseBuilder AddHeader(string name, string value)
        {
            headers.Add(name, value);

            return this;
        }
        public IHttpResponseBuilder AddHeader(HttpResponseHeader header, string value)
        {
            return AddHeader(header.ToString(), value);
        }
        public IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            foreach (var k in headers)
            {
                headers.Add(k.Key, k.Value);
            }

            return this;
        }

        public IHttpResponseBuilder SetHeaderContentEncoding(string contentEncoding)
        {
            return AddHeader(HttpResponseHeader.ContentEncoding, contentEncoding);
        }

        public IHttpResponseBuilder SetHeaderContentLength(long contentLength)
        {
            return AddHeader(HttpResponseHeader.ContentLength, contentLength.ToString());
        }

        public IHttpResponseBuilder SetHeaderConnection(string connectionStatus)
        {
            return AddHeader(HttpResponseHeader.Connection, connectionStatus);
        }
        public IHttpResponseBuilder AddCookie(IEnumerable<HttpCookie> cookies)
        {
            foreach (var cookie in cookies)
            {
                this.cookies[cookie.Name] = cookie;
            }

            return this;
        }

        public IHttpResponseBuilder AddCookie(HttpCookie cookie)
        {
            cookies[cookie.Name] = cookie;

            return this;
        }
    }
}
