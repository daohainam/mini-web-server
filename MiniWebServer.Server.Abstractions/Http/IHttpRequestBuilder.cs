using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions.Http
{
    public interface IHttpRequestBuilder
    {
        HttpRequest Build();

        IHttpRequestBuilder SetMethod(MiniWebServer.Abstractions.Http.HttpMethod method);
        IHttpRequestBuilder SetUrl(string url);
        IHttpRequestBuilder AddHeader(string name, string value);
        IHttpRequestBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> headers);
        IHttpRequestBuilder AddTransferEncoding(string transferEncodings);
        IHttpRequestBuilder SetParameters(HttpParameters parameters);
        IHttpRequestBuilder SetQueryString(string queryString);
        IHttpRequestBuilder SetHash(string hash);
        IHttpRequestBuilder SetBodyReader(PipeReader reader);
        IHttpRequestBuilder AddCookie(IEnumerable<HttpCookie> cookies);
    }
}
