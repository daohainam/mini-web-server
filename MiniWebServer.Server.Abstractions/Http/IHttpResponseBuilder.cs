using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions.Http
{
    public interface IHttpResponseBuilder
    {
        HttpResponse Build();
        IHttpResponseBuilder SetStatusCode(HttpStatusCode httpStatusCode);
        IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues);
        IHttpResponseBuilder AddHeader(string name, string value);
        IHttpResponseBuilder AddHeader(HttpResponseHeader header, string value);
        IHttpResponseBuilder SetReasonPhrase(string message);
        IHttpResponseBuilder SetContent(MiniApp.MiniContent content);
        IHttpResponseBuilder AddCookie(IEnumerable<HttpCookie> cookies);
        IHttpResponseBuilder SetHeaderContentEncoding(string contentEncoding);
        IHttpResponseBuilder SetHeaderContentLength(long contentLength);
        IHttpResponseBuilder SetHeaderConnection(string connectionStatus);
    }
}
