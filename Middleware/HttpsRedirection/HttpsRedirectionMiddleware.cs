using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;

namespace MiniWebServer.HttpsRedirection
{
    public class HttpsRedirectionMiddleware : IMiddleware
    {
        private readonly HttpsRedirectionOptions options;
        private readonly ILogger<HttpsRedirectionMiddleware> logger;

        public HttpsRedirectionMiddleware(HttpsRedirectionOptions options, ILogger<HttpsRedirectionMiddleware>? logger)
        {
            this.options = options;
            this.logger = logger ?? NullLogger<HttpsRedirectionMiddleware>.Instance;
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            if (context.Request.IsHttps)
            {
                await next.InvokeAsync(context, cancellationToken);
            }
            else
            {
                logger.LogInformation("Redirecting request to HTTPS...");

                context.Response.StatusCode = Abstractions.HttpResponseCodes.TemporaryRedirect;

                var redirectUrl = BuildRedirectUrl("https", context.Request.Host, options.HttpsPort, context.Request.Url);

                context.Response.Headers.Location = redirectUrl;

                return;
            }
        }

        private static string BuildRedirectUrl(string protocol, string host, int httpsPort, string url)
        {
            if (httpsPort != 443)
            {
                host += ":" + httpsPort;
            }
            return protocol + "://" + host + url;
        }
    }
}