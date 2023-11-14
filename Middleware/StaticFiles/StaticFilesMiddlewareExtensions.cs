using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Server.Abstractions;

namespace MiniWebServer.StaticFiles
{
    public static class StaticFilesMiddlewareExtensions
    {
        public static void UseStaticFiles(this IMiniAppBuilder appBuilder, string root, long defaultMaxAge = 0)
        {
            StaticFilesOptions options = new(root, cacheOptions: new StaticFilesCacheOptions(defaultMaxAge));

            appBuilder.Services.AddTransient(services => new StaticFilesMiddleware(
                options,
                services.GetService<IMimeTypeMapping>(),
                services.GetService<ILoggerFactory>()
                ));

            appBuilder.UseMiddleware<StaticFilesMiddleware>();
        }

        public static void UseStaticFiles(this IMiniAppBuilder appBuilder, StaticFilesOptions options)
        {
            appBuilder.Services.AddTransient(services => new StaticFilesMiddleware(
                options,
                services.GetService<IMimeTypeMapping>(),
                services.GetService<ILoggerFactory>()
                ));

            appBuilder.UseMiddleware<StaticFilesMiddleware>();
        }
    }
}
