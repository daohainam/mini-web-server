using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    public class DefaultWebSocketAppBuilder : IWebSocketAppBuilder
    {
        private readonly Dictionary<string, Func<IWebSocket, Task>> handlers = [];

        public IWebSocketAppBuilder AddHandler(string route, Func<IWebSocket, Task> handler)
        {
            handlers[route] = handler;

            return this;
        }
        public IWebSocketAppBuilder AddHandler(string route, IWebSocketHandler handler)
        {
            handlers[route] = handler.InvokeAsync;

            return this;
        }

        public Func<IWebSocket, Task>? FindHandler(string route)
        {
            return handlers[route];
        }
    }
}
