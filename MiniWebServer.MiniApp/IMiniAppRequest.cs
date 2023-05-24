using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppRequest
    {
        IAppContext Context { get; }
        string Url { get; }
        PipeReader? BodyReader { get; }
        HttpCookies Cookies { get; }
        string Hash { get; }
        HttpHeaders Headers { get; }
        Abstractions.Http.HttpMethod Method { get; }
        HttpParameters QueryParameters { get; }
        string QueryString { get; }
    }
}
