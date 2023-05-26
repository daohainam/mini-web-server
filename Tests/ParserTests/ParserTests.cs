using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Abstractions.HttpParser.Http11;

namespace ParserTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        [DataRow("GET /index.html?p1=v1&p2=v2&txt=Mini20Web20Server&d=d1&d=d2&d=d3#hash HTTP/1.1", "GET", "/index.html", "#hash", "?p1=v1&p2=v2&txt=Mini20Web20Server&d=d1&d=d2&d=d3", "1", "1", 4)]
        [DataRow("GET /index.html?fbclid=IwAR2bODvpBU9VB9t8-kyhqN5XEzLcbV1IfjIwmYmYbrD86W7NUd4aUAnyf9k HTTP/1.1", "GET", "/index.html", "", "?fbclid=IwAR2bODvpBU9VB9t8-kyhqN5XEzLcbV1IfjIwmYmYbrD86W7NUd4aUAnyf9k", "1", "1", 1)]
        public void IsValidRequestLine(string text, string method, string url, string hash, string queryString, string majorVersion, string minorVersion, int paramsCount)
        {
            IHttp11Parser http11Parser = new RegexHttp11Parsers();

            var result = http11Parser.ParseRequestLine(text);
            Assert.IsNotNull(result);
            Assert.AreEqual(method, result.Method);
            Assert.AreEqual(url, result.Url);
            Assert.AreEqual(hash, result.Hash);
            Assert.AreEqual(queryString, result.QueryString);
            Assert.AreEqual(majorVersion, result.ProtocolVersion.Major);
            Assert.AreEqual(minorVersion, result.ProtocolVersion.Minor);
            Assert.AreEqual(paramsCount, result.Parameters.Count);
        }

        [TestMethod]
        [DataRow("Host: www.example.com", "Host", "www.example.com")]
        [DataRow("Accept-Language: en", "Accept-Language", "en")]
        [DataRow("Accept-Encoding: gzip, deflate, br", "Accept-Encoding", "gzip, deflate, br")]
        [DataRow("Connection: keep-alive", "Connection", "keep-alive")]
        public void IsValidHeaderLine(string text, string name, string value)
        {
            IHttp11Parser http11Parser = new RegexHttp11Parsers();

            var result = http11Parser.ParseHeaderLine(text);
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(value, result.Value);
        }
    }
}