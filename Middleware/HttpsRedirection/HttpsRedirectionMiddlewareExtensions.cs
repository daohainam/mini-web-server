using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.HttpsRedirection
{
    public static class HttpsRedirectionMiddlewareExtensions
    {
        public static void UseHttpsRedirection(this IMiniAppBuilder appBuilder, Action<HttpsRedirectionOptions>? action = default)
        {
            var options = new HttpsRedirectionOptions();
            action?.Invoke(options);

            appBuilder.Services.AddTransient(services => new HttpsRedirectionMiddleware(
                options, services.GetService<ILogger<HttpsRedirectionMiddleware>>()
                )
            );

            appBuilder.UseMiddleware<HttpsRedirectionMiddleware>();
        }
    }
}
