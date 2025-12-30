using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;

namespace MiniWebServer.OutputCaching;

internal class OutputCachingMiddleware : IMiddleware
{
    private readonly ILogger<OutputCachingMiddleware> logger;
    private readonly OutputCachingOptions options;
    private readonly IOutputCacheKeyGenerator outputCacheKeyGenerator;
    private readonly IOutputCacheStorage outputCacheStorage;

    public OutputCachingMiddleware(OutputCachingOptions options, IOutputCacheKeyGenerator outputCacheKeyGenerator, IOutputCacheStorage outputCacheStorage, ILogger<OutputCachingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;
        this.outputCacheKeyGenerator = outputCacheKeyGenerator ?? throw new ArgumentNullException(nameof(outputCacheKeyGenerator));
        this.outputCacheStorage = outputCacheStorage ?? throw new ArgumentNullException(nameof(outputCacheStorage));

        this.logger = logger;
    }
    public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
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

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error matching: {url}", url);
            }
        }

        if (policy != null)
        {
            var cacheKey = outputCacheKeyGenerator.GenerateCacheKey(context);
            var cachedStream = outputCacheStorage.GetCachedStream(cacheKey);

            if (cachedStream != null)
            {
                context.Response.Content = cachedStream.Content;
                context.Response.StatusCode = cachedStream.StatusCode;
                context.Response.Headers.AddOrUpdate(cachedStream.Headers);
            }
            else
            {
                await next.InvokeAsync(context, cancellationToken);

                // now we will store output to cache storage
            }
        }
        else
        {
            await next.InvokeAsync(context, cancellationToken);
        }
    }
}
