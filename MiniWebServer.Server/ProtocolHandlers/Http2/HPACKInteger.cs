using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class HPACKInteger
    {
        private const byte IS_HUFFMAN_ENCODED_BITS = 0b_1000_0000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Decode(int v, int n)
        {
            // https://httpwg.org/specs/rfc7541.html#integer.representation
            return (int)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Pow2(int n) // 2 ^ n
        {
            if (n == 0)
                return 1;
            if (n == 1) 
                return 2;
            return 2 << (n - 1);
        }

        public static int ReadInt(ref ReadOnlySequence<byte> payload, int n)
        {
            var hs = payload.Slice(0, 1).FirstSpan[0];
            payload = payload.Slice(1);
            
            if (hs < Pow2(n) - 1)
            {
                return hs;
            }
            else
            {
                var m = 0;
                int i = hs;
                byte next;
                do
                {
                    next = payload.Slice(0, 1).FirstSpan[0];
                    payload = payload.Slice(1);

                    i += (next & 0b_0111_1111) * Pow2(m);

                    m += 7;
                } while ((next & 0b_1000_0000) == 0b_1000_0000); // bit [0] == 1

                return i;
            }
        }
        public  static int ReadStringLength(ref ReadOnlySequence<byte> payload, out bool isHuffmanEncoded)
        {
            var n = payload.Slice(0, 1).FirstSpan[0];

            isHuffmanEncoded = (n & IS_HUFFMAN_ENCODED_BITS) == IS_HUFFMAN_ENCODED_BITS;

            return ReadInt(ref payload, 7);
        }

        public static void WriteInt(int hs, byte[] bytes, int n, out int length)
        {
            length = 0;

            int pow = Pow2(n) - 1;
            if (hs < pow)
            {
                bytes[length++] = (byte)hs;
            }
            else
            {
                bytes[length] = (byte)pow;
                hs -= pow;

                while (hs >= 128)
                {
                    bytes[length++] = (byte)(0b_1000_0000 | (hs % 128));
                    hs /= 128;
                }
                bytes[length++] = (byte)hs;
            }
        }
    }
}
