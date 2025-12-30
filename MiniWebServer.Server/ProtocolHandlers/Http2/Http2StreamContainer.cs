using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

internal class Http2StreamContainer: ConcurrentDictionary<uint, Http2Stream>
{
}
