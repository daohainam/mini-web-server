using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class DefaultWebSocketManager : IWebSocketManager
    {
        public bool IsUpgradeRequest { get; set; } = false;
        public Func<IWebSocket, Task>? Handler { get; set; } = default;
    }
}
