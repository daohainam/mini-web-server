using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2FrameWriter
    {
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

        public static int SerializeSettingFrame(Http2Frame frame, IEnumerable<Http2FrameSETTINGSItem> settingItems, byte[] writePayload)
        {
            ArgumentNullException.ThrowIfNull(nameof(frame));

            if (writePayload.Length < (9 + (settingItems.Count() * 6)))
            {
                throw new InternalHttp2Exception("Payload size too small");
            }

            int length = 0;

            WritePayloadLength(writePayload, length);
            writePayload[3] = (byte)Http2FrameType.SETTINGS;
            writePayload[4] = (byte)frame.Flags;
            // note: SETTINGS frames always have stream Id == 0
            writePayload[5] = 0; // TODO: can we use SIMD instructions here? 
            writePayload[6] = 0;
            writePayload[7] = 0;
            writePayload[8] = 0;

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
    }
}