using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class DefaultWebSocketFactory : IWebSocketFactory
    {
        public IWebSocket CreateWebSocket(Stream inStream, Stream outStream)
        {
            return new WebSocketImpl(inStream, outStream);
        }
    }
}
