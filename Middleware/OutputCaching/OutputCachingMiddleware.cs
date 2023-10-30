using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;

namespace MiniWebServer.OutputCaching
{
    internal class OutputCachingMiddleware : IMiddleware
    {
        private readonly ILogger<OutputCachingMiddleware> logger;
        private readonly OutputCachingOptions options;

        public OutputCachingMiddleware(OutputCachingOptions options, ILoggerFactory? loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.options = options;

            logger = loggerFactory != null ? loggerFactory.CreateLogger<OutputCachingMiddleware>() : NullLogger<OutputCachingMiddleware>.Instance;
        }
        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            string url = context.Request.Url;

            IOutputCachePolicy? policy = null;

            foreach (var p in options.Policies)  
            {
                try
                {
                    var matched = p.PathMatching(url)
                        && p.Methods.Contains(context.Request.Method);

                    if (matched)
                    {
                        policy = p;
                        break;
                    }

                } catch (Exception ex)
                {
                    logger.LogError(ex, "Error matching: {url}", url);
                }
            }

            if (policy != null)
            {
                await next.InvokeAsync(context, cancellationToken);

                // now we will store output to cache storage
            }
            else
            {
                await next.InvokeAsync(context, cancellationToken);
            }
        }
    }
}