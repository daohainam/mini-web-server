using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket.Abstractions
{
    public interface IWebSocketFactory
    {
        IWebSocket CreateWebSocket(Stream inStream, Stream outStream);
    }
}
