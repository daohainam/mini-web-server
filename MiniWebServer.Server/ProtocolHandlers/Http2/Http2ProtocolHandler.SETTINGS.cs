using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public partial class Http2ProtocolHandler
    {
        private bool ProcessSETTINGSFrame(ref Http2Frame frame, ref ReadOnlySequence<byte> payload, out Http2FrameSETTINGSItem[] settings)
        {
            if (frame.StreamIdentifier != 0)
            {
                logger.LogError("Stream identifier for a SETTINGS frame MUST be zero");
                settings = [];
                return false;
            }

            if (!Http2FrameReader.TryReadSETTINGSFramePayload(ref payload, out settings))
            {
                return false;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Found {n} settings in payload", settings.Length);

                foreach (var key in settings)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("{id}: {v}", key.Identifier, key.Value);
                    }

                    if (!ProcessSETTINGSItem(key.Identifier, key.Value))
                    {
                        return false;
                    }
                }
            }

            if (frame.Flags.HasFlag(Http2FrameFlags.ACK))
            {
                logger.LogDebug("Received a SETTINGS_ACK frame");
            }
            else
            {
                // send an ACK back
                var settingFrame = new Http2Frame()
                {
                    FrameType = Http2FrameType.SETTINGS,
                    Flags = Http2FrameFlags.ACK
                };

                var writePayload = ArrayPool<byte>.Shared.Rent((int)maxFrameSize);
                int length = Http2FrameWriter.SerializeSETTINGSFrame(settingFrame, [], writePayload);
                if (length > 0)
                {
                    protocolHandlerContext.Stream.Write(writePayload, 0, length);
                }

                ArrayPool<byte>.Shared.Return(writePayload);
            }

            return true;
        }

        private bool ProcessSETTINGSItem(Http2FrameSettings identifier, uint value)
        {
            switch (identifier)
            {
                case Http2FrameSettings.SETTINGS_ENABLE_PUSH:
                    if (value != 0)
                    {
                        logger.LogInformation("Server Push is not supported"); // PUSH_PROMISE is not supported (it is deprecated by Chrome)
                    }
                    break;
                case Http2FrameSettings.SETTINGS_MAX_CONCURRENT_STREAMS:
                    logger.LogInformation("Max_concurrent_streams = {m}", value); // 0 is also accepted, but it will prevent creating new streams
                    maxConcurrentStreams = value;
                    break;
                case Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE:
                    if (value >= 16_383 && value <= 16_777_215)
                    {
                        logger.LogInformation("MAX_FRAME_SIZE = {m}", value);
                        maxFrameSize = value;
                    }
                    else
                    {
                        logger.LogError("Invalid MAX_FRAME_SIZE: {m}", value);
                        return false;
                    }
                    break;
                case Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE:
                    if (value >= 65_535 && value <= 2_147_643_647) // Values above the maximum flow-control window size of (2^31)-1 MUST be treated as a connection error
                    {
                        logger.LogInformation("INITIAL_WINDOW_SIZE = {m}", value);
                        initialWindowSize = value;
                    }
                    else
                    {
                        logger.LogError("Invalid INITIAL_WINDOW_SIZE: {m}", value);
                        return false;
                    }
                    break;
                case Http2FrameSettings.SETTINGS_HEADER_TABLE_SIZE:
                    if (value >= 1_024 && value <= 16_384) 
                    {
                        logger.LogInformation("HEADER_TABLE_SIZE = {m}", value);
                        headerTableSize = value;
                    }
                    else
                    {
                        logger.LogError("Invalid HEADER_TABLE_SIZE: {m}", value);
                        return false;
                    }
                    break;
            }

            return true;
        }
    }
}
