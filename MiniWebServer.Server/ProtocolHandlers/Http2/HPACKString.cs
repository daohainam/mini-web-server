using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKString
    {
        public static string Decode(bool isHuffmanEncoded, ReadOnlySpan<byte> span)
        {
            if (isHuffmanEncoded)
            {
                throw new InvalidOperationException("Huffman encode not supported");
            }
            else
            {
                return Encoding.ASCII.GetString(span);
            }
        }
    }
}
