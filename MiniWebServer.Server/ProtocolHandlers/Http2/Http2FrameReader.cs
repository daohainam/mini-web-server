using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2FrameReader
    {
        public static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out Http2Frame? frame)
        {
            frame = null; // should we allocate new frames or reuse?

            return false;
        }
    }
}
