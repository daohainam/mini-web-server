using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket.Abstractions
{
    public interface IWebSocketManager
    {
        bool IsUpgradeRequest { get; set; }
        Func<IWebSocket, Task>? Handler { get; set; }
    }
}
