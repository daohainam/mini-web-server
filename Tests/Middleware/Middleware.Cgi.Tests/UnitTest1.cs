using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Cgi;
using MiniWebServer.MiniApp;
using MvcMiddlewareTests;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace Middleware.Cgi.Tests
{
    [TestClass]
    public class UnitTest1: ICallable
    {
        public Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task TestMethod1Async()
        {
            var options = new CgiOptions()
            {
                Handlers = [
                    new CgiHandler()
                    {
                        Executable = "C:\\PHP\\php.exe",
                        Route = "/cgi-bin/hellocgi.php",
                        WorkingDirectory = ".",
                        ScriptDirectory = ".\\scripts\\php",                        
                    }
                ]
            };
            var middleware = new CgiMiddleware(options, NullLogger.Instance);
            var context = BuildContext("/cgi-bin/hellocgi.php", "");
            await middleware.InvokeAsync(context, this, default);

            Assert.AreEqual(HttpResponseCodes.OK, context.Response.StatusCode);

            var memoryStream = new MemoryStream();
            await context.Response.Content.WriteToAsync(memoryStream, default);
            string result = Encoding.UTF8.GetString(new ReadOnlySequence<byte>(memoryStream.GetBuffer()).Slice(0, memoryStream.Length));
            Assert.AreEqual("Hello CGI!\r\n", result);
        }

        private FakeMiniAppContext BuildContext(string url, string queryString)
        {
            var pipe = new Pipe();

            var context = new FakeMiniAppContext(() => {
                return new HttpRequest(
                    1,
                    MiniWebServer.Abstractions.Http.HttpMethod.Get,
                    "www.mini-web-server.com",
                    443,
                    url,
                    [],
                    queryString,
                    "",
                    [],
                    [], 
                    [],
                    pipe,
                    0, 
                    "text/html",
                    true,
                    System.Net.IPAddress.Parse("127.0.0.1"),
                    443,
                    HttpVersions.Http11
                    );
            },
            () => {
                var stream = new MemoryStream();
                return new HttpResponse(HttpResponseCodes.NotFound, stream)
                {
                };
            }
            )
            {
            };

            return context;
        }
    }
}