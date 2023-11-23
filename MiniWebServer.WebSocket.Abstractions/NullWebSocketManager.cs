using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket.Abstractions
{
    public class NullWebSocketManager : IWebSocketManager
    {
        public bool IsUpgradeRequest { get => false; set => throw new InvalidOperationException(); }
        public static NullWebSocketManager Instance { get; } = new NullWebSocketManager();
        Func<IWebSocket, Task>? IWebSocketManager.Handler { get => null; set => throw new InvalidOperationException(); }
    }
}
