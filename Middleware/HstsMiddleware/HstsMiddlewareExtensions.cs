using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp.Builders;

namespace MiniWebServer.HstsMiddleware
{
    public static class HstsMiddlewareExtensions
    {
        public static void UseHttpsRedirection(this IMiniAppBuilder appBuilder, Action<HstsOptions>? action = default)
        {
            var options = new HstsOptions();
            action?.Invoke(options);

            appBuilder.Services.AddTransient(services => new HstsMiddleware(
                options
                )
            );

            appBuilder.UseMiddleware<HstsMiddleware>();
        }
    }
}
