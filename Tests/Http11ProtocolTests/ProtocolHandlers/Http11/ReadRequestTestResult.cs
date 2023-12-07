using MiniWebServer.Server.Http;

namespace Http11ProtocolTests.ProtocolHandlers.Http11
{
    internal class ReadRequestTestResult(bool success, HttpWebRequestBuilder httpWebRequestBuilder)
    {
        public bool Success { get; set; } = success;
        public HttpWebRequestBuilder HttpWebRequestBuilder { get; set; } = httpWebRequestBuilder;
    }
}
