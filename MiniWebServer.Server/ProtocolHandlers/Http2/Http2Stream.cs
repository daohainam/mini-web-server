using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2Stream
    {
        public uint StreamId { get; set; }
        public Http2StreamStates State { get; set; } = Http2StreamStates.IDLE;
        public required StreamFrameQueue FrameQueue { get; set; }
    }
}
