using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket.Abstractions
{
    public class WebSocketAcceptOptions
    {
        public WebSocketDeflateOptions? WebSocketDeflateOptions { get; set; }
    }
}
