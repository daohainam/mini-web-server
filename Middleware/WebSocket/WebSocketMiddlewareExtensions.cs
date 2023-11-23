using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.WebSocket;
using MiniWebServer.WebSocket.Abstractions;

namespace MiniWebServer.Session
{
    public static class WebSocketMiddlewareExtensions
    {
        public static void AddWebSocketService(this IServiceCollection services, Action<WebSocketOptions>? configureOptions = default)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            var options = new WebSocketOptions();
            configureOptions?.Invoke(options);

            services.AddTransient<IWebSocketManager>(services => new DefaultWebSocketManager());
            services.AddTransient<IWebSocketFactory>(services => new DefaultWebSocketFactory());

            services.AddTransient(services => new WebSocketMiddleware(
                options,
                services.GetRequiredService<ILogger<WebSocketMiddleware>>(),
                services.GetRequiredService<DefaultWebSocketAppBuilder>()
                ));
        }

        public static IWebSocketAppBuilder UseWebSockets(this IMiniAppBuilder appBuilder)
        {
            ArgumentNullException.ThrowIfNull(appBuilder, nameof(appBuilder));

            var wsappBuilder = new DefaultWebSocketAppBuilder();

            appBuilder.Services.AddTransient(services => wsappBuilder);
            appBuilder.UseMiddleware<WebSocketMiddleware>();

            return wsappBuilder;
        }
    }
}
