using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2Stream
    {
        public required uint StreamId { get; set; }
        public Http2StreamStates State { get; set; } = Http2StreamStates.IDLE;
        public required StreamFrameQueue FrameQueue { get; set; }
        public List<Http2FrameHEADERSPayload> HeaderPayloads { get; set; } = []; // we use a separate list for header payloads, this helps speeding up processing, but consumes more memory, consider again if we target small devices
    }
}
