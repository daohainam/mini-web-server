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

        internal static int SerializeHEADERFrame(uint streamId, string key, IEnumerable<string> values, bool isContinuation, bool isEndOfHeader, byte[] writePayload)
        {
            throw new NotImplementedException();
        }
    }
}