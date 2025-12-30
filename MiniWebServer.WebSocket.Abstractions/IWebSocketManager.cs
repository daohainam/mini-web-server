using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket.Abstractions;

public interface IWebSocketManager
{
    bool IsUpgradeRequest { get; }
    Task<System.Net.WebSockets.WebSocket> AcceptAsync(WebSocketAcceptOptions? acceptOptions = default, CancellationToken cancellationToken = default);
}
