using MiniWebServer.Abstractions;
using MiniWebServer.WebSocket.Abstractions;
using System.Security.Claims;

namespace MiniWebServer.MiniApp;

public interface IMiniAppContext
{
    ISession Session { get; set; }
    IServiceProvider Services { get; }
    ClaimsPrincipal? User { get; set; }
    IWebSocketManager WebSockets { get; set; }
}
