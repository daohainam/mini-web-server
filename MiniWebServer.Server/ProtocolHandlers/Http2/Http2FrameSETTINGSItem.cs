using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2FrameSETTINGSItem // https://datatracker.ietf.org/doc/html/rfc9113#section-6.5
    {
        public Http2FrameSettings Identifier { get; set; }
        public uint Value { get; set; }
    }
}
