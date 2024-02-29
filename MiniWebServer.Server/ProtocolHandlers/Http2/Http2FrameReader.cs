using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2FrameReader
    {
        private const int HeaderLength = 9; // From RFC: All frames begin with a fixed 9-octet header

        public static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out Http2Frame? frame, int maxFrameSize)
        {
            frame = null; // should we allocate new frames or reuse?

            if (buffer.Length < HeaderLength)
            { 
                return false; 
            }

            var headerSlice = buffer.Slice(0, HeaderLength);
            var header = headerSlice.FirstSpan; // hope that everything is in the first span :)
            var headerLength = ReadHeaderLength(header);

            if (headerLength > maxFrameSize)
            {
                throw new Http2Exception($"FrameSize error {headerLength}");
            }

            return false;
        }

        public static int ReadHeaderLength(ReadOnlySpan<byte> header)
        {
            return header[0] << 16 | header[1] << 8 | header[2];
        }
    }
}
