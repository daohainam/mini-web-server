using Http2.Hpack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hpack = global::Http2.Hpack;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKString
    {
        public static string Decode(bool isHuffmanEncoded, ReadOnlySequence<byte> span)
        {
            if (isHuffmanEncoded)
            {
                var n = Huffman.Decode(new ArraySegment<byte>(span.ToArray()), ArrayPool<byte>.Shared);

                return n;

                //throw new InvalidOperationException("Huffman encode not supported");
            }
            else
            {
                return Encoding.ASCII.GetString(span);
            }
        }
    }
}
