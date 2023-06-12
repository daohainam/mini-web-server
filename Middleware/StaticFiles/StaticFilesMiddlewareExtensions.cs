using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.StaticFiles
{
    public static class StaticFilesMiddlewareExtensions
    {
        public static void UseStaticFiles(this IMiniAppBuilder appBuilder, string root)
        {
            StaticFilesOptions options = new()
            {
                Root = root ?? "wwwroot"
            };

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
                options.Root,
                options.DefaultDocuments,
                services.GetService<IMimeTypeMapping>(),
                services.GetService<ILoggerFactory>()
                ));

            appBuilder.UseMiddleware<StaticFilesMiddleware>();
        }
    }
}
