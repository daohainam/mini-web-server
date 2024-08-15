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
                var decoder = new Hpack.StringDecoder(4096, ArrayPool<byte>.Shared);
                var n = decoder.Decode(new ArraySegment<byte>(span.ToArray()));

                return decoder.Result;

                //throw new InvalidOperationException("Huffman encode not supported");
            }
            else
            {
                return Encoding.ASCII.GetString(span);
            }
        }
    }
}
