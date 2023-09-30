using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public static class MvcMiddlewareExtensions
    {
        public static void UseMvc(this IMiniAppBuilder appBuilder, MvcOptions? options = default)
        {
            options ??= new MvcOptions();

            appBuilder.Services.AddTransient(services => new MvcMiddleware(
                options,
                services.GetRequiredService<ILoggerFactory>(),
                appBuilder.Services
                ));

            appBuilder.UseMiddleware<MvcMiddleware>();
        }
    }
}
