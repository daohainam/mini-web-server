using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.HstsMiddleware
{
    public static class HstsMiddlewareExtensions
    {
        public static void UseHttpsRedirection(this IMiniAppBuilder appBuilder, Action<HstsOptions>? action = default)
        {
            var options = new HstsOptions();
            action?.Invoke(options);

            appBuilder.Services.AddTransient(services => new HstsMiddleware(
                options, services.GetService<ILogger<HstsMiddleware>>()
                )
            );

            appBuilder.UseMiddleware<HstsMiddleware>();
        }
    }
}
