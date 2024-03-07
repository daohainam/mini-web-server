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

        public static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, ref Http2Frame frame, int maxFrameSize, out ReadOnlySequence<byte> payload)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(frame);

            payload = ReadOnlySequence<byte>.Empty;
            if (buffer.Length < HeaderLength)
            { 
                return false; 
            }

            var headerSlice = buffer.Slice(0, HeaderLength);
            var header = headerSlice.FirstSpan; // hope that everything is in the first span :)
            var payloadLength = ReadPayloadLength(header);

            if (payloadLength > maxFrameSize)
            {
                throw new Http2Exception($"FrameSize error {payloadLength}");
            }

            if (buffer.Length < HeaderLength + payloadLength)
            {
                return false;
            }

            frame.Length = payloadLength;
            frame.FrameType = GetFrameType(header[3]);
            frame.Flags = header[4];
            frame.StreamIdentifier = GetStreamIdentifier(header);

            payload = buffer.Slice(HeaderLength, payloadLength);
            buffer = buffer.Slice(payload.End);

            return true;
        }

        private static int GetStreamIdentifier(ReadOnlySpan<byte> header)
        {
            return (header[5] & 0b_0111_1111) << 24 | header[6] << 16 | header[7] << 8 | header[8];
        }

        private static Http2FrameType GetFrameType(byte b) => b switch
        {
            0 => Http2FrameType.DATA,
            1 => Http2FrameType.HEADERS,
            2 => Http2FrameType.PRIORITY,
            3 => Http2FrameType.RST_STREAM,
            4 => Http2FrameType.SETTINGS,
            5 => Http2FrameType.PUSH_PROMISE,
            6 => Http2FrameType.PING,
            7 => Http2FrameType.GOAWAY,
            8 => Http2FrameType.WINDOW_UPDATE,
            9 => Http2FrameType.CONTINUATION,
            _ => throw new Http2Exception($"Unknown frame type {b}")
        };

        public static int ReadPayloadLength(ReadOnlySpan<byte> header)
        {
            return header[0] << 16 | header[1] << 8 | header[2];
        }
    }
}
