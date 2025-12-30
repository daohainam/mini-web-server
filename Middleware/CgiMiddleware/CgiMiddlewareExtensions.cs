using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Cgi.Parsers;
using MiniWebServer.MiniApp.Builders;
using System.Reflection;

namespace MiniWebServer.Cgi;

public static class CgiMiddlewareExtensions
{
    public static void AddCgiService(this IServiceCollection services, Action<CgiOptions>? configureOptions = default)
    {
        var options = new CgiOptions(
                            );

        configureOptions?.Invoke(options);

        services.AddTransient(services => new CgiMiddleware(
            options,
            services.GetRequiredService<ILogger<CgiMiddleware>>()
            ));
    }

    public static void UseMvc(this IMiniAppBuilder appBuilder)
    {
        appBuilder.UseMiddleware<CgiMiddleware>();
    }

}
