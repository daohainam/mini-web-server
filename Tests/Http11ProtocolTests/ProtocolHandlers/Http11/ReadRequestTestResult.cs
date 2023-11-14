using MiniWebServer.Server.Http;

namespace Http11ProtocolTests.ProtocolHandlers.Http11
{
    internal class ReadRequestTestResult
    {
        public ReadRequestTestResult(bool success, HttpWebRequestBuilder httpWebRequestBuilder)
        {
            Success = success;
            HttpWebRequestBuilder = httpWebRequestBuilder;
        }

        public bool Success { get; set; }
        public HttpWebRequestBuilder HttpWebRequestBuilder { get; set; }
    }
}
