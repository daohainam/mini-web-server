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
        string Url { get; }
        IMiniBodyManager BodyManager { get; }
        HttpCookies Cookies { get; }
        string Hash { get; }
        HttpRequestHeaders Headers { get; }
        Abstractions.Http.HttpMethod Method { get; }
        HttpParameters QueryParameters { get; }
        string QueryString { get; }
        long ContentLength { get; }
        string ContentType { get; }

        Task<IRequestForm> ReadFormAsync(CancellationToken cancellationToken = default);
    }
}
