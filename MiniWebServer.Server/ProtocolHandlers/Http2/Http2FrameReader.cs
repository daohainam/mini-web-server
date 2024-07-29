using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var header = headerSlice.IsSingleSegment ? headerSlice.FirstSpan : headerSlice.ToArray(); 
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
            frame.Flags = (Http2FrameFlags)header[4];
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

        public static bool TryReadPINGFramePayload(ref ReadOnlySequence<byte> payload, out byte[] opaqueData)
        {
            if (payload.Length != 8) // https://httpwg.org/specs/rfc9113.html#PING: Receipt of a PING frame with a length field value other than 8 MUST be treated as a connection error
            {
                opaqueData = [];
                return false;
            }

            opaqueData = new byte[8];
            payload.CopyTo(opaqueData);

            return true;
        }

        public static bool TryReadPRIORITYFramePayload(ref ReadOnlySequence<byte> payload, out Http2FramePRIOTITYPayload priorityPayload)
        {
            if (payload.Length != 5)
            {
                priorityPayload = Http2FramePRIOTITYPayload.Empty;
                return false;
            }

            var payloadBytes = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();
            priorityPayload = new() { 
                Exclusive = (payloadBytes[0] & 0b_1000_0000) != 0,
                StreamDependency = (uint)((payloadBytes[0] & 0b_0111_1111) << 24 | payloadBytes[1] << 16 | payloadBytes[2] << 8 | payloadBytes[3]),
                Weight = payloadBytes[4]
            };

            return true;
        }

        public static bool TryReadHEADERSFramePayload(ref Http2Frame frame, ReadOnlySequence<byte> payload, HPACKHeaderTable headerTable, out Http2FrameHEADERSPayload headersPayload, ILogger logger)
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

            headersPayload = new();

            // it is easier to parse the header frames when we have received a full stream, but I decided to parse here to determine errors as soon as possible

            while (!payload.IsEmpty)
            {
                //var payloadBytes = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();
                var bb = payload.ToArray();
                byte b = payload.Slice(0, 1).FirstSpan[0];

                if ((b & 0b_1000_0000) == 0b_1000_0000) // Indexed Header Field, https://httpwg.org/specs/rfc7541.html#indexed.header.representation
                {
                    payload = payload.Slice(1);

                    var headerIndex = b & 0x7F;
                    var header = HPACKStaticTable.GetHeader(headerIndex);

                    if (header == null)
                    {
#if DEBUG
                        logger.LogError("Header index not found {idx}", headerIndex);
#endif
                        headersPayload = Http2FrameHEADERSPayload.Empty;
                        return false;
                    }
                    else
                    {
#if DEBUG
                        logger.LogDebug("Static header: {k}: {v}", header.Name, header.Value);
#endif
                        headersPayload.Headers.Add(header);
                    }
                }
                else if ((b & 0b_1100_0000) == 0b_0100_0000) // Literal Header Field with Incremental Indexing, https://httpwg.org/specs/rfc7541.html#literal.header.with.incremental.indexing
                {
                    var index = b & 0b_0011_1111;
                    if (index == 0) // New Name
                    {
                        /*
                          0   1   2   3   4   5   6   7
                        +---+---+---+---+---+---+---+---+
                        | 0 | 1 |           0           |
                        +---+---+-----------------------+
                        | H |     Name Length (7+)      |
                        +---+---------------------------+
                        |  Name String (Length octets)  |
                        +---+---------------------------+
                        | H |     Value Length (7+)     |
                        +---+---------------------------+
                        | Value String (Length octets)  |
                        +-------------------------------+
                        Literal Header Field with Incremental Indexing — New Name 
                         */

                        payload = payload.Slice(1);

                        var length = HPACKInteger.ReadStringLength(ref payload, out var isHuffmanEncoded);
                        var name = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, length)));

                        payload = payload.Slice(length);
                        length = HPACKInteger.ReadStringLength(ref payload, out isHuffmanEncoded);

                        length = HPACKInteger.ReadStringLength(ref payload, out isHuffmanEncoded);
                        var value = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, length)));

#if DEBUG
                        logger.LogDebug("New LwII header: {k}: {v}", name, value);
#endif
                        headersPayload.Headers.Add(new HPACKHeader(name, value));

                        payload = payload.Slice(length);
                    }
                    else
                    {
                        /*
                          0   1   2   3   4   5   6   7
                        +---+---+---+---+---+---+---+---+
                        | 0 | 1 |      Index (6+)       |
                        +---+---+-----------------------+
                        | H |     Value Length (7+)     |
                        +---+---------------------------+
                        | Value String (Length octets)  |
                        +-------------------------------+

                        Literal Header Field with Incremental Indexing — Indexed Name
                        */

                        //index = HPACKInteger.ReadInt(ref payload, 8);
                        
                        payload = payload.Slice(1); 

                        // read from header tables
                        var valueLength = HPACKInteger.ReadStringLength(ref payload, out var isHuffmanEncoded);
                        var value = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, valueLength)));

                        if (headerTable.TryGetHeader(index, out var header))
                        {
#if DEBUG
                            logger.LogDebug("Indexed LwII header: {k}: {v}", header!.Name, value);
#endif
                            headersPayload.Headers.Add(new HPACKHeader(header.HeaderType, header.StaticTableIndex, header.Name, value));
                        }

                        payload = payload.Slice(valueLength);
                    }
                }
                else if ((b & 0b_1111_0000) == 0b_0000_0000) // Literal Header Field without Incremental Indexing, https://httpwg.org/specs/rfc7541.html#literal.header.without.indexing
                {
                    var index = b & 0b_0000_1111;
                    if (index == 0) // New Name
                    {
                        /*
                              0   1   2   3   4   5   6   7
                            +---+---+---+---+---+---+---+---+
                            | 0 | 0 | 0 | 0 |       0       |
                            +---+---+-----------------------+
                            | H |     Name Length (7+)      |
                            +---+---------------------------+
                            |  Name String (Length octets)  |
                            +---+---------------------------+
                            | H |     Value Length (7+)     |
                            +---+---------------------------+
                            | Value String (Length octets)  |
                            +-------------------------------+
                            Literal Header Field without Indexing — New Name
                         */

                        var length = HPACKInteger.ReadStringLength(ref payload, out var isHuffmanEncoded);
                        var name = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, length)));

                        payload = payload.Slice(length);
                        length = HPACKInteger.ReadStringLength(ref payload, out isHuffmanEncoded);

                        length = HPACKInteger.ReadStringLength(ref payload, out isHuffmanEncoded);
                        var value = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, length)));

#if DEBUG
                        logger.LogDebug("New LwoII header: {k}: {v}", name, value);
#endif
                        headersPayload.Headers.Add(new HPACKHeader(name, value));

                        payload = payload.Slice(length);
                    }
                    else
                    {
                        /*
                            Literal Header Field without Indexing — Indexed Name

                            0   1   2   3   4   5   6   7
                            +---+---+---+---+---+---+---+---+
                            | 0 | 0 | 0 | 0 |  Index (4+)   |
                            +---+---+-----------------------+
                            | H |     Value Length (7+)     |
                            +---+---------------------------+
                            | Value String (Length octets)  |
                            +-------------------------------+
                        */

                        index = HPACKInteger.ReadInt(ref payload, 4);

                        // read from header tables
                        var valueLength = HPACKInteger.ReadStringLength(ref payload, out var isHuffmanEncoded);
                        var value = HPACKString.Decode(isHuffmanEncoded, GetSpan(payload.Slice(0, valueLength)));

                        if (headerTable.TryGetHeader(index, out var header))
                        {
#if DEBUG
                            logger.LogDebug("Indexed LwoII header: {k}: {v}", header!.Name, value);
#endif
                            headersPayload.Headers.Add(new HPACKHeader(header.HeaderType, header.StaticTableIndex, header.Name, value));
                        }

                        payload = payload.Slice(valueLength);
                    }
                }
                else if ((b & 0b_1110_0000) == 0b_0010_0000) // Dynamic Table Size Update, https://httpwg.org/specs/rfc7541.html#encoding.context.update
                {

                }
            }

            var flags = frame.Flags;
            int idx = 0;

            var hasPayLength = flags.HasFlag(Http2FrameFlags.PADDED);
            var hasPriority = flags.HasFlag(Http2FrameFlags.PRIORITY);

            //if (hasPayLength)
            //{
            //    headersPayload.PadLength = payloadBytes[0];

            //    idx = Interlocked.Increment(ref idx);
            //}
            //else
            //{
            //    headersPayload.PadLength = 0;
            //}

            //if (hasPriority)
            //{
            //    headersPayload.Exclusive = (payloadBytes[idx] & 0b_1000_0000) != 0;
            //    headersPayload.StreamDependency = (uint)((payloadBytes[idx] & 0b_0111_1111) << 24 | payloadBytes[idx + 1] << 16 | payloadBytes[idx + 2] << 8 | payloadBytes[idx + 3]);
            //    headersPayload.Weight = payloadBytes[idx + 4];

            //    idx = Interlocked.Add(ref idx, 5);
            //}
            //else
            //{
            //    headersPayload.Exclusive = false;
            //    headersPayload.StreamDependency = 0;
            //    headersPayload.Weight = 0;
            //}

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

        private static ReadOnlySpan<byte> GetSpan(ReadOnlySequence<byte> readOnlySequence)
        {
            return readOnlySequence.IsSingleSegment ? readOnlySequence.FirstSpan : readOnlySequence.ToArray();
        }

        private static bool TryParseFieldBlock(ReadOnlySequence<byte> readOnlySequence, out Http2Fields fields)
        {
            var s = Encoding.ASCII.GetString(readOnlySequence.ToArray());

            fields = new Http2Fields();

            return true;
        }

        
    }
}
