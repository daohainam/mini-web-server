using Http11ProtocolTests;
using Http11ProtocolTests.ProtocolHandlers.Http11;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.Tests
{
    [TestClass()]
    public class ReadRequestTests
    {
        [TestMethod()]
        public async Task ReadRequestTestAsync()
        {
            string requestContent =
                @"GET /index.html?id1=1&id2=2&t1=Mini%20Web%20Server HTTP/1.1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36
Host: localhost:8443
Connection: keep-alive
Cookie: CONSENT=PENDING+243; sw_version=1; JSESSION=10F4DF1D-C4BE-44BA-AF88-81A3AC132E6A
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Accept-Encoding: gzip, deflate, br
Accept-Language: vi,en-US;q=0.9,en;q=0.8,nb;q=0.7
Cache-Control:no-cache

";

            var result = await ReadRequestAsync(requestContent);

            Assert.AreEqual(true, result.Success);

            var request = result.HttpWebRequestBuilder.Build();
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(request.Url, "/index.html");
            Assert.AreEqual(request.Headers.Host, "localhost:8443");
            Assert.AreEqual(request.Headers.Connection, "keep-alive");
            Assert.AreEqual(request.Headers.AcceptLanguage, "vi,en-US;q=0.9,en;q=0.8,nb;q=0.7");
            Assert.AreEqual(request.Headers.CacheControl, "no-cache");
        }

        private static async Task<ReadRequestTestResult> ReadRequestAsync(string requestContent)
        {
            var reader = PipeUtils.String2Reader(requestContent);

            var http11Parser = new RegexHttp11Parsers();
            var handler = new Http11IProtocolHandler(new ProtocolHandlerConfiguration(ProtocolHandlerFactory.HTTP11, 1024 * 1024 * 10), new LoggerFactory(), http11Parser);

            var requestBuilder = new HttpWebRequestBuilder();

            var result = await handler.ReadRequestAsync(reader, requestBuilder, CancellationToken.None);

            return new ReadRequestTestResult(result, requestBuilder);
        }


    }
}