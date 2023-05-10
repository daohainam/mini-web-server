using Http11ProtocolTests.ProtocolHandlers.Http11;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.HttpParser.Http11;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.ProtocolHandlers.Http11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.Tests
{
    [TestClass()]
    public class ReadRequestTests
    {
        [TestMethod()]
        public async Task ReadRequestTest()
        {
            string requestContent =
                @"GET /index.html HTTP/1.1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36
Host: localhost:8443
Connection: keep-alive
Cookie: CONSENT=PENDING+243; sw_version=1; JSESSION=10F4DF1D-C4BE-44BA-AF88-81A3AC132E6A
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Accept-Encoding: gzip, deflate, br
Accept-Language: vi,en-US;q=0.9,en;q=0.8,nb;q=0.7
Cache-Control:no-cache

";

            var result = await ReadRequest(requestContent);

            Assert.AreEqual(ProtocolHandlerStates.BuildRequestStates.Succeeded, result.State);

            var request = result.HttpWebRequestBuilder.Build();
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(request.Url, "/index.html");
            Assert.AreEqual(request.Headers.Host, "localhost:8443");
            Assert.AreEqual(request.Headers.Connection, "keep-alive");
            Assert.AreEqual(request.Headers.AcceptLanguage, "vi,en-US;q=0.9,en;q=0.8,nb;q=0.7");
            Assert.AreEqual(request.Headers.CacheControl, "no-cache");
        }

        private async Task<ReadRequestTestResult> ReadRequest(string requestContent)
        {
            using var stream = String2Stream(requestContent);
            var http11Parser = new RegexHttp11Parsers();
            var handler = new Http11IProtocolHandler(NullLogger.Instance, http11Parser);

            var requestBuilder = new HttpWebRequestBuilder();
            var protocolData = new ProtocolHandlerData();

            var result = await handler.ReadRequestAsync(stream, requestBuilder, protocolData);
            while (result == ProtocolHandlerStates.BuildRequestStates.InProgressWithNoData)
            {
                result = await handler.ReadRequestAsync(stream, requestBuilder, protocolData);
            }

            return new ReadRequestTestResult(result, requestBuilder);
        }

        public static Stream String2Stream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}