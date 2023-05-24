using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IHttpResponse
    {
        HttpStatusCode StatusCode { get; }
        string ReasonPhrase { get; }
        HttpResponseHeaders Headers { get; }
        IHttpContent Content { get; }
        HttpCookies Cookies { get; }
    }
}
