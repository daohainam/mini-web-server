using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public interface IHttpRequestBuilder
    {
        IHttpRequestBuilder SetMethod(HttpMethod method);
        IHttpRequestBuilder SetUrl(string url);
        IHttpRequestBuilder AddHeader(string name, string value);
        IHttpRequestBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> headers);
        IHttpRequestBuilder AddTransferEncoding(string transferEncodings);
        HttpRequest Build();
    }
}
