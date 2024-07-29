using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Huffman
    {
        // encode/decode data based on https://httpwg.org/specs/rfc7541.html#huffman.code
        internal static object Decode(ReadOnlySpan<byte> span, out string s)
        {
            throw new NotImplementedException();
        }
    }
}
