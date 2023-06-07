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
    public static class StaticFilesHelpers
    {
        public static void UseStaticFiles(this IMiniAppBuilder appBuilder, string root)
        {
            root ??= "wwwroot";

            appBuilder.Services.AddTransient(services => new StaticFilesMiddleware(
                root,
                services.GetService<IMimeTypeMapping>(),
                services.GetService<ILoggerFactory>()
                ));

            appBuilder.UseMiddleware<StaticFilesMiddleware>();
        }
    }
}
