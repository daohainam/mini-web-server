using MiniWebServer.Abstractions;
using MiniWebServer.Server.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
