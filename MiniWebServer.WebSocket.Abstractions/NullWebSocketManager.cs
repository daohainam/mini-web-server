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

        public Task<System.Net.WebSockets.WebSocket> AcceptAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException();
        }
    }
}
