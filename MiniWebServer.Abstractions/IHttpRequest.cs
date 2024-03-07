using MiniWebServer.Abstractions.Http;
using System.IO.Pipelines;
using System.Net;

namespace MiniWebServer.Abstractions
{
    public interface IHttpRequest : IParametersContainer, IRequestHeadersContainer, IFormContainer, IRequestBodyReader
    {
        Http.HttpMethod Method { get; }
        string Host { get; }
        int Port { get; }
        string Url { get; }
        bool KeepAliveRequested { get; }
        HttpCookies Cookies { get; }
        Pipe BodyPipeline { get; }
        string Hash { get; }
        string QueryString { get; }
        long ContentLength { get; }
        string ContentType { get; }
        string[] Segments { get; }
        bool IsHttps { get; }
        public IRequestBodyManager BodyManager { get; }
        IPAddress? RemoteAddress { get; }
        int RemotePort { get; }
        HttpVersions HttpVersion { get; }
    }

    public enum HttpVersions
    {
        Http11 = 101,
        Http20 = 200,
        Http30 = 300,
    }
}
