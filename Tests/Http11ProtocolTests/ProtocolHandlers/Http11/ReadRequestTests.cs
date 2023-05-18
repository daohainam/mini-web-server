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
        public void ReadRequestTest()
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

            var result = ReadRequest(requestContent);

            Assert.AreEqual(ProtocolHandlerStates.BuildRequestStates.Succeeded, result.State);

            var request = result.HttpWebRequestBuilder.Build();
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(request.Url, "/index.html");
            Assert.AreEqual(request.Headers.Host, "localhost:8443");
            Assert.AreEqual(request.Headers.Connection, "keep-alive");
            Assert.AreEqual(request.Headers.AcceptLanguage, "vi,en-US;q=0.9,en;q=0.8,nb;q=0.7");
            Assert.AreEqual(request.Headers.CacheControl, "no-cache");
        }

        private ReadRequestTestResult ReadRequest(string requestContent)
        {
            var memory = String2Span(requestContent);

            var http11Parser = new RegexHttp11Parsers();
            var handler = new Http11IProtocolHandler(NullLogger.Instance, http11Parser);

            var requestBuilder = new HttpWebRequestBuilder();
            var protocolData = new ProtocolHandlerData();
            int bp = 0;

            var result = handler.ReadRequest(memory[bp..].Span, requestBuilder, protocolData, out bp);
            while (result == ProtocolHandlerStates.BuildRequestStates.InProgress)
            {
                result = handler.ReadRequest(memory[bp..].Span, requestBuilder, protocolData, out bp);
            }

            return new ReadRequestTestResult(result, requestBuilder);
        }

        public static Memory<byte> String2Span(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream.GetBuffer().AsMemory();
        }
    }
}