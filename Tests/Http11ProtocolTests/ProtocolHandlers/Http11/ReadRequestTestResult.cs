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
        public ReadRequestTestResult(ProtocolHandlerStates.BuildRequestStates state, HttpWebRequestBuilder httpWebRequestBuilder)
        {
            State = state;
            HttpWebRequestBuilder = httpWebRequestBuilder;
        }

        public ProtocolHandlerStates.BuildRequestStates State { get; set; } 
        public HttpWebRequestBuilder HttpWebRequestBuilder { get; set; }
    }
}
