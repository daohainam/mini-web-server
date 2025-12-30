using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.WebSocket;
using MiniWebServer.WebSocket.Abstractions;
using System.Runtime.CompilerServices;

namespace MiniWebServer.Session;

public static class WebSocketMiddlewareExtensions
{
    public static void AddWebSocketService(this IServiceCollection services, Action<WebSocketOptions>? configureOptions = default)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var options = new WebSocketOptions();
        configureOptions?.Invoke(options);

        services.AddTransient(services => new WebSocketMiddleware(
            options,
            services.GetRequiredService<ILogger<WebSocketMiddleware>>()
            ));
    }

    public static IMiniAppBuilder UseWebSockets(this IMiniAppBuilder appBuilder)
    {
        ArgumentNullException.ThrowIfNull(appBuilder, nameof(appBuilder));

        appBuilder.UseMiddleware<WebSocketMiddleware>();

        return appBuilder;
    }
}
