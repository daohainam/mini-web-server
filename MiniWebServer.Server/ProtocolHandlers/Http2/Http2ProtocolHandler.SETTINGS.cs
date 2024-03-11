using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public partial class Http2ProtocolHandler
    {
        private bool ProcessSETTINGSFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload)
        {
            if (frame.StreamIdentifier != 0)
            {
                logger.LogError("Stream identifier for a SETTINGS frame MUST be zero");
                return false;
            }

            if (!Http2FrameReader.TryReadSETTINGSFramePayload(ref payload, out var settings))
            {
                return false;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Found {n} settings in payload", settings.Length);

                foreach (var key in settings)
                {
                    logger.LogDebug("{id}: {v}", key.Identifier, key.Value);
                }
            }

            return true;
        }
    }
}
