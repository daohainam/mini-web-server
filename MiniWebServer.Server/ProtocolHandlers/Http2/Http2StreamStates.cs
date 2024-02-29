using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal enum Http2StreamStates
    {
        IDLE,
        RESERVED,
        OPEN,
        HALF_CLOSED,
        CLOSED
    }
}
