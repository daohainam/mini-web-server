using Http11ProtocolTests;
using Http11ProtocolTests.ProtocolHandlers.Http11;
using Microsoft.Extensions.Logging;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Cookie;
using MiniWebServer.Server.Http;
using HttpMethod = global::MiniWebServer.Abstractions.Http.HttpMethod;

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

        [TestMethod()]
        [DataRow("GET /index.html?id1=1&id2=2&t1=Mini%20Web%20Server HTTP/1.1\r\n", "GET", "/index.html")]
        [DataRow("GET /this/is/a/path? HTTP/1.1\r\n", "GET", "/this/is/a/path")]
        public async Task RequestLineParserTestAsync(string requestLine, string method, string url)
        {
            var result = await ReadRequestAsync(requestLine);

            Assert.AreEqual(true, result.Success);

            var request = result.HttpWebRequestBuilder.Build();
            Assert.AreEqual(method, request.Method.Method);
            Assert.AreEqual(url, request.Url);
        }


        private static async Task<ReadRequestTestResult> ReadRequestAsync(string requestContent)
        {
            var reader = PipeUtils.String2Reader(requestContent);
            var loggerFactory = new LoggerFactory();

            var httpParser = new ByteSequenceHttpParser(loggerFactory);
            var handler = new Http11IProtocolHandler(new ProtocolHandlerConfiguration(ProtocolHandlerFactory.HTTP11, 1024 * 1024 * 10), loggerFactory, httpParser, new DefaultCookieParser());

            var requestBuilder = new HttpWebRequestBuilder();

            var result = await handler.ReadRequestAsync(reader, requestBuilder, CancellationToken.None);

            return new ReadRequestTestResult(result, requestBuilder);
        }


    }
}