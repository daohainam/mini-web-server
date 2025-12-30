using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2FrameWriter
    {
        private const int PING_OPAQUE_DATA_SIZE = 8;

        internal static Task<bool> SerializeHeaderFrames(uint streamId, IHttpResponse response, Stream stream)
        {
            throw new NotImplementedException();
        }

        //internal static async Task WriteFrameAsync(Stream stream, ulong http2StreamId, Http2Frame frame, byte[] writePayload)
        //{
        //    ArgumentNullException.ThrowIfNull(nameof(stream));
        //    ArgumentNullException.ThrowIfNull(nameof(frame));

        //    switch (frame.FrameType) { 
        //        case Http2FrameType.SETTINGS:
        //            var b = SerializeSettingFrame(stream, http2StreamId, frame, writePayload);
        //    }
        //}

        private static void WritePayloadLength(byte[] payload, int length)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(payload.Length, 3);

            payload[0] = (byte)((length >> 16) & 0xFF);
            payload[1] = (byte)((length >> 8) & 0xFF);
            payload[2] = (byte)(length & 0xFF);
        }

        public static int SerializeSETTINGSFrame(Http2Frame frame, IEnumerable<Http2FrameSETTINGSItem> settingItems, byte[] writePayload)
        {
            ArgumentNullException.ThrowIfNull(nameof(frame));

            if (writePayload.Length < (9 + (settingItems.Count() * 6)))
            {
                throw new InternalHttp2Exception("Payload size too small");
            }

            int length = settingItems.Count() * 6; // 6 == setting item size

            WritePayloadLength(writePayload, length);
            writePayload[3] = (byte)Http2FrameType.SETTINGS;
            writePayload[4] = (byte)frame.Flags;
            // note: SETTINGS frames always have stream Id == 0
            Array.Clear(writePayload, 5, 4);

            int idx = 9;
            foreach (var settingItem in settingItems) {
                writePayload[idx++] = (byte)(((ushort)settingItem.Identifier >> 8) & 0xFF);
                writePayload[idx++] = (byte)((ushort)settingItem.Identifier & 0xFF);

                writePayload[idx++] = (byte)(((ushort)settingItem.Value >> 24) & 0xFF);
                writePayload[idx++] = (byte)(((ushort)settingItem.Value >> 16) & 0xFF);
                writePayload[idx++] = (byte)(((ushort)settingItem.Value >> 8) & 0xFF);
                writePayload[idx++] = (byte)((ushort)settingItem.Value & 0xFF);
            }

            return idx;
        }

        public static int SerializePINGFrame(Http2Frame frame, byte[] opaqueData, byte[] writePayload)
        {
            ArgumentNullException.ThrowIfNull(nameof(frame));

            if (opaqueData.Length != PING_OPAQUE_DATA_SIZE)
            {
                throw new InternalHttp2Exception("Invalid OpaqueData size");
            }

            WritePayloadLength(writePayload, 8);
            writePayload[3] = (byte)Http2FrameType.PING;
            writePayload[4] = (byte)frame.Flags;
            // note: PING frames always have stream Id == 0
            Array.Clear(writePayload, 5, 4);
            Array.Copy(opaqueData, 0, writePayload, 9, PING_OPAQUE_DATA_SIZE);

            return 9 + PING_OPAQUE_DATA_SIZE; // header size + opaque data size
        }

        //internal static int SerializeHEADERFrame(ulong streamId, IEnumerable<HttpHeader> headers, byte[] writePayload)
        //{
        //    ArgumentNullException.ThrowIfNull(nameof(frame));
        //    ArgumentNullException.ThrowIfNull(nameof(headers));
        //    ArgumentNullException.ThrowIfNull(nameof(writePayload));

        //    int payLoadLength = 0;
        //    foreach (HttpHeader header in headers) { 

        //    }

        //    WritePayloadLength(writePayload, payLoadLength);
        //}

        internal static int SerializeHEADERFrame(uint streamId, string key, IEnumerable<string> values, bool isFirst, bool isLast, byte[] writePayload)
        {
            ArgumentNullException.ThrowIfNull(writePayload);

            if (writePayload.Length < 9)
            {
                throw new InternalHttp2Exception("Payload size too small");
            }

            // Build the header field in a temporary buffer
            var headerFieldBuffer = new byte[writePayload.Length - 9];
            int headerFieldLength = 0;

            // Encode the header using HPACK literal header field without indexing
            // Pattern: 0000xxxx where xxxx is the name index (or 0 for new name)
            
            // Check if this is a known header name in the static table
            int nameIndex = GetHeaderNameIndex(key);
            
            if (nameIndex > 0)
            {
                // Indexed name - Literal Header Field without Indexing
                // Pattern: 0000xxxx where xxxx is the index (using 4-bit prefix)
                if (nameIndex < 15)
                {
                    headerFieldBuffer[headerFieldLength++] = (byte)nameIndex;
                }
                else
                {
                    // Use HPACK integer encoding for larger indices
                    headerFieldBuffer[headerFieldLength++] = 0x0F; // Set lower 4 bits to 1111
                    int remaining = nameIndex - 15;
                    while (remaining >= 128)
                    {
                        headerFieldBuffer[headerFieldLength++] = (byte)(0x80 | (remaining % 128));
                        remaining /= 128;
                    }
                    headerFieldBuffer[headerFieldLength++] = (byte)remaining;
                }
            }
            else
            {
                // New name - Literal Header Field without Indexing
                headerFieldBuffer[headerFieldLength++] = 0x00;
                
                // Encode name as string (with length prefix)
                byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(key.ToLower());
                if (nameBytes.Length < 127)
                {
                    headerFieldBuffer[headerFieldLength++] = (byte)nameBytes.Length;
                }
                else
                {
                    // Use HPACK integer encoding for large strings
                    headerFieldBuffer[headerFieldLength++] = 0x7F;
                    int remaining = nameBytes.Length - 127;
                    while (remaining >= 128)
                    {
                        headerFieldBuffer[headerFieldLength++] = (byte)(0x80 | (remaining % 128));
                        remaining /= 128;
                    }
                    headerFieldBuffer[headerFieldLength++] = (byte)remaining;
                }
                Array.Copy(nameBytes, 0, headerFieldBuffer, headerFieldLength, nameBytes.Length);
                headerFieldLength += nameBytes.Length;
            }

            // Encode value as string (with length prefix)
            string value = string.Join(", ", values);
            byte[] valueBytes = System.Text.Encoding.ASCII.GetBytes(value);
            if (valueBytes.Length < 127)
            {
                headerFieldBuffer[headerFieldLength++] = (byte)valueBytes.Length;
            }
            else
            {
                // Use HPACK integer encoding for large strings
                headerFieldBuffer[headerFieldLength++] = 0x7F;
                int remaining = valueBytes.Length - 127;
                while (remaining >= 128)
                {
                    headerFieldBuffer[headerFieldLength++] = (byte)(0x80 | (remaining % 128));
                    remaining /= 128;
                }
                headerFieldBuffer[headerFieldLength++] = (byte)remaining;
            }
            Array.Copy(valueBytes, 0, headerFieldBuffer, headerFieldLength, valueBytes.Length);
            headerFieldLength += valueBytes.Length;

            // Write frame header
            WritePayloadLength(writePayload, headerFieldLength);
            writePayload[3] = (byte)Http2FrameType.HEADERS;
            
            // Set flags
            Http2FrameFlags flags = Http2FrameFlags.NONE;
            if (isLast)
            {
                flags |= Http2FrameFlags.END_HEADERS;
            }
            writePayload[4] = (byte)flags;

            // Write stream identifier
            writePayload[5] = (byte)((streamId >> 24) & 0x7F); // First bit must be 0
            writePayload[6] = (byte)((streamId >> 16) & 0xFF);
            writePayload[7] = (byte)((streamId >> 8) & 0xFF);
            writePayload[8] = (byte)(streamId & 0xFF);

            // Copy header field data
            Array.Copy(headerFieldBuffer, 0, writePayload, 9, headerFieldLength);

            return 9 + headerFieldLength;
        }

        private static int GetHeaderNameIndex(string name)
        {
            // Return static table index for common header names
            // Reference: https://httpwg.org/specs/rfc7541.html#static.table.definition
            return name.ToLower() switch
            {
                "accept-charset" => 15,
                "accept-encoding" => 16,
                "accept-language" => 17,
                "accept-ranges" => 18,
                "accept" => 19,
                "access-control-allow-origin" => 20,
                "age" => 21,
                "allow" => 22,
                "authorization" => 23,
                "cache-control" => 24,
                "content-disposition" => 25,
                "content-encoding" => 26,
                "content-language" => 27,
                "content-length" => 28,
                "content-location" => 29,
                "content-range" => 30,
                "content-type" => 31,
                "cookie" => 32,
                "date" => 33,
                "etag" => 34,
                "expect" => 35,
                "expires" => 36,
                "from" => 37,
                "host" => 38,
                "if-match" => 39,
                "if-modified-since" => 40,
                "if-none-match" => 41,
                "if-range" => 42,
                "if-unmodified-since" => 43,
                "last-modified" => 44,
                "link" => 45,
                "location" => 46,
                "max-forwards" => 47,
                "proxy-authenticate" => 48,
                "proxy-authorization" => 49,
                "range" => 50,
                "referer" => 51,
                "refresh" => 52,
                "retry-after" => 53,
                "server" => 54,
                "set-cookie" => 55,
                "strict-transport-security" => 56,
                "transfer-encoding" => 57,
                "user-agent" => 58,
                "vary" => 59,
                "via" => 60,
                "www-authenticate" => 61,
                _ => 0
            };
        }

        internal static int SerializeDATAFrame(uint streamId, byte[] data, int offset, int length, bool endStream, byte[] writePayload)
        {
            ArgumentNullException.ThrowIfNull(writePayload);
            ArgumentNullException.ThrowIfNull(data);

            if (writePayload.Length < 9 + length)
            {
                throw new InternalHttp2Exception("Payload size too small");
            }

            // Write frame header
            WritePayloadLength(writePayload, length);
            writePayload[3] = (byte)Http2FrameType.DATA;
            
            // Set flags
            Http2FrameFlags flags = Http2FrameFlags.NONE;
            if (endStream)
            {
                flags |= Http2FrameFlags.END_STREAM;
            }
            writePayload[4] = (byte)flags;

            // Write stream identifier
            writePayload[5] = (byte)((streamId >> 24) & 0x7F); // First bit must be 0
            writePayload[6] = (byte)((streamId >> 16) & 0xFF);
            writePayload[7] = (byte)((streamId >> 8) & 0xFF);
            writePayload[8] = (byte)(streamId & 0xFF);

            // Copy data
            if (length > 0)
            {
                Array.Copy(data, offset, writePayload, 9, length);
            }

            return 9 + length;
        }
    }
}