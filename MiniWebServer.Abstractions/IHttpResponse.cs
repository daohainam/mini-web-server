using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IHttpResponse
    {
        HttpResponseCodes StatusCode { get; set; }
        string ReasonPhrase { get; set; }
        HttpResponseHeaders Headers { get; }
        IHttpContent Content { get; set; }
        HttpCookies Cookies { get; }
        Stream Body { get; set; }
    }
}
