using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public interface IHttpResponseBuilder
    {
        HttpResponse Build();
        IHttpResponseBuilder SetStatusCode(HttpStatusCode httpStatusCode);
        IHttpResponseBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> keyValues);
        IHttpResponseBuilder AddHeader(string name, string value);
        IHttpResponseBuilder SetReasonPhrase(string message);
        IHttpResponseBuilder SetContent(HttpContent content);
    }
}
