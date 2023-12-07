using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.WebSocket.Abstractions;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppConnectionContext(IServiceProvider serviceProvider)
    {
        public IServiceProvider Services { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        public ILoggerFactory LoggerFactory { get; } = serviceProvider.GetRequiredService<ILoggerFactory>();
        public IWebSocketManager WebSockets { get; set; } = serviceProvider.GetService<IWebSocketManager>() ?? NullWebSocketManager.Instance;
    }
}
