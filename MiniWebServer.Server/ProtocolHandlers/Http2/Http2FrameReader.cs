using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2FrameReader
    {
        private const int HeaderLength = 9; // From RFC: All frames begin with a fixed 9-octet header

        public static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, ref Http2Frame frame, uint maxFrameSize, out ReadOnlySequence<byte> payload)
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

        private static uint GetStreamIdentifier(ReadOnlySpan<byte> header)
        {
            // first bit is always 0 so it is always a positive number
            return (uint)((header[5] & 0b_0111_1111) << 24 | header[6] << 16 | header[7] << 8 | header[8]);
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

        public static bool TryReadSETTINGSFramePayload(ref ReadOnlySequence<byte> payload, out Http2FrameSETTINGSItem[] settings)
        {
            if (payload.Length % 6 != 0)
            {
                settings = [];
                return false;
            }

            var payloadBytes = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();

            int count = payloadBytes.Length / 6;
            settings = new Http2FrameSETTINGSItem[count];
            for (int i = 0; i < count; i++)
            {
                int si = i * 6;
                settings[i] = new Http2FrameSETTINGSItem()
                {
                    Identifier = (Http2FrameSettings)(payloadBytes[si] << 8 | payloadBytes[si + 1]),
                    Value = (uint)(payloadBytes[si + 2] << 24 | payloadBytes[si + 3] << 16 | payloadBytes[si + 4] << 8 | payloadBytes[si + 5])
                };
            }

            return true;
        }

        public static bool TryReadWINDOW_UPDATEFramePayload(ref ReadOnlySequence<byte> payload, out uint windowSizeIncrement)
        {
            if (payload.Length != 4)
            {
                windowSizeIncrement = 0;
                return false;
            }

            var payloadBytes = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();

            windowSizeIncrement = (uint)((payloadBytes[0] & 0b_0111_1111) << 24 | payloadBytes[1] << 16 | payloadBytes[2] << 8 | payloadBytes[3]);

            return true;
        }

        public static bool TryReadHEADERSFramePayload(ref Http2Frame frame, ref ReadOnlySequence<byte> payload, out Http2FrameHEADERSPayload headersPayload)
        {
            /*
              HEADERS Frame {
              Length (24),
              Type (8) = 0x01,

              Unused Flags (2),
              PRIORITY Flag (1),
              Unused Flag (1),
              PADDED Flag (1),
              END_HEADERS Flag (1),
              Unused Flag (1),
              END_STREAM Flag (1),

              Reserved (1),
              Stream Identifier (31),

              [Pad Length (8)],
              [Exclusive (1)],
              [Stream Dependency (31)],
              [Weight (8)],
              Field Block Fragment (..),
              Padding (..2040),
            }
             */

            var payloadBytes = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();
            var flags = frame.Flags;
            int idx = 0;
            headersPayload = new();

            var hasPayLength = (flags & 0b_0000_1000) != 0;
            var hasPriority = (flags & 0b_0010_0000) != 0;
            var hasEndHeaders = (flags & 0b_0000_0100) != 0;
            var hasEndStream = (flags & 0b_0000_0001) != 0;

            if (hasPayLength)
            {
                headersPayload.PadLength = payloadBytes[0];

                idx = Interlocked.Increment(ref idx);
            }
            else
            {
                headersPayload.PadLength = 0;
            }

            if (hasPriority)
            {
                headersPayload.Exclusive = (payloadBytes[idx] & 0b_1000_0000) != 0;
                headersPayload.StreamDependency = (uint)((payloadBytes[idx] & 0b_0111_1111) << 24 | payloadBytes[idx + 1] << 16 | payloadBytes[idx + 2] << 8 | payloadBytes[idx + 3]);
                headersPayload.Weight = payloadBytes[idx + 4];

                idx = Interlocked.Add(ref idx, 5);
            }
            else
            {
                headersPayload.Exclusive = false;
                headersPayload.StreamDependency = 0;
                headersPayload.Weight = 0;
            }

            if (TryParseFieldBlock(payload.Slice(idx), out var fieldBlock))
            {
                headersPayload.FieldBlockFragment = fieldBlock;
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool TryParseFieldBlock(ReadOnlySequence<byte> readOnlySequence, out Http2Fields fields)
        {
            var s = Encoding.ASCII.GetString(readOnlySequence.ToArray());

            fields = new Http2Fields();

            return true;
        }
    }
}
