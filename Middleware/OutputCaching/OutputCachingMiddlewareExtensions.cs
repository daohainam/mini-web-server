using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.OutputCaching.CacheStorage;

namespace MiniWebServer.OutputCaching;

public static class OutputCachingMiddlewareExtensions
{
    public static void UseOutputCache(this IMiniAppBuilder appBuilder, OutputCachingOptions options)
    {
        appBuilder.Services.AddTransient(services => new OutputCachingMiddleware(
            options,
            new DefaultOutputCacheKeyGenerator(),
            options.OutputCacheStorage ?? new DistributeCacheStorage(services.GetRequiredService<IDistributedCache>()),
            services.GetRequiredService<ILogger<OutputCachingMiddleware>>()
            ));

        appBuilder.UseMiddleware<OutputCachingMiddleware>();
    }
}
