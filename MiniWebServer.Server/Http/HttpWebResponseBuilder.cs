//using MiniWebServer.Abstractions;
//using MiniWebServer.Abstractions.Http;
//using MiniWebServer.MiniApp;
//using MiniWebServer.Server.Abstractions.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace MiniWebServer.Server.Http
//{
//    public class HttpWebResponseBuilder : IHttpResponseBuilder, IHttpResponse
//    {
//        private HttpResponseCodes statusCode = HttpResponseCodes.InternalServerError;
//        private readonly HttpResponseHeaders headers = new();
//        private string reasonPhrase = string.Empty;
//        private readonly HttpCookies cookies = new();
//        private MiniContent content = new global::MiniWebServer.MiniApp.Content.StringContent(string.Empty);

//        public HttpResponseCodes StatusCode => statusCode;

//        public string ReasonPhrase => reasonPhrase;

//        public HttpResponseHeaders Headers => headers;

//        public IHttpContent Content => content;

//        public HttpCookies Cookies => cookies;

//        public HttpResponse Build()
//        {
//            headers.ContentLength = content.ContentLength.ToString();
//            headers.AddOrSkip("Content-Type", "text/plain");
//            headers.AddOrSkip("Server", "Mini-Web-Server/alpha");

//            var response = new HttpResponse(statusCode, reasonPhrase, headers, cookies, content);

//            return response;
//        }

//        public IHttpResponseBuilder SetStatusCode(HttpResponseCodes httpStatusCode)
//        {
//            statusCode = httpStatusCode;

//            return this;
//        }

//        public IHttpResponseBuilder SetReasonPhrase(string reasonPhrase)
//        {
//            this.reasonPhrase = reasonPhrase ?? string.Empty;

//            return this;
//        }

//        public IHttpResponseBuilder SetContent(MiniContent content)
//        {
//            this.content = content ?? throw new ArgumentNullException(nameof(content));

//            return this;
//        }

//        public IHttpResponseBuilder AddHeader(string name, string value)
//        {
//            headers.Add(name, value);
//            return this;
//        }
//        public IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues)
//        {
//            foreach (var k in headers)
//            {
//                headers.Add(k.Value);
//            }
//            return this;
//        }
//        public IHttpResponseBuilder AddOrUpdateHeader(string name, string value)
//        {
//            headers.AddOrUpdate(name, value);
//            return this;
//        }
//        public IHttpResponseBuilder AddOrSkipHeader(string name, string value)
//        {
//            headers.AddOrSkip(name, value);
//            return this;
//        }

//        public IHttpResponseBuilder SetHeaderContentEncoding(string contentEncoding)
//        {
//            headers.ContentEncoding = contentEncoding;

//            return this;
//        }

//        public IHttpResponseBuilder SetHeaderContentLength(long contentLength)
//        {
//            headers.ContentLength = contentLength.ToString();

//            return this;
//        }

//        public IHttpResponseBuilder SetHeaderConnection(string connectionStatus)
//        {
//            headers.Connection = connectionStatus;

//            return this;
//        }
//        public IHttpResponseBuilder AddCookie(IEnumerable<HttpCookie> cookies)
//        {
//            foreach (var cookie in cookies)
//            {
//                this.cookies[cookie.Name] = cookie;
//            }

//            return this;
//        }

//        public IHttpResponseBuilder AddCookie(HttpCookie cookie)
//        {
//            cookies[cookie.Name] = cookie;

//            return this;
//        }

//    }
//}
