using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2StreamContainer: Dictionary<uint, Http2Stream>
    {
    }
}
