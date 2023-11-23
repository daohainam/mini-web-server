using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    public interface IWebSocketAppBuilder
    {
        IWebSocketAppBuilder AddHandler(string route, Func<IWebSocket, Task> handler);
    }
}
