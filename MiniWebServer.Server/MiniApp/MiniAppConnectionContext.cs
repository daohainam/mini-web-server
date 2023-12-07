using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.WebSocket.Abstractions;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppConnectionContext
    {
        public MiniAppConnectionContext(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            WebSockets = serviceProvider.GetService<IWebSocketManager>() ?? NullWebSocketManager.Instance;
        }

        public IServiceProvider Services { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IWebSocketManager WebSockets { get; set; }
    }
}
