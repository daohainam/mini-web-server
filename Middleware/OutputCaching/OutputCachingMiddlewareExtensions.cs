using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.OutputCaching
{
    public static class OutputCachingMiddlewareExtensions
    {
        public static void UseOutputCache(this IMiniAppBuilder appBuilder, OutputCachingOptions options)
        {
            appBuilder.Services.AddTransient(services => new OutputCachingMiddleware(
                options,
                new DefaultOutputCacheKeyGenerator(),
                services.GetService<ILoggerFactory>()
                ));

            appBuilder.UseMiddleware<OutputCachingMiddleware>();
        }
    }
}
