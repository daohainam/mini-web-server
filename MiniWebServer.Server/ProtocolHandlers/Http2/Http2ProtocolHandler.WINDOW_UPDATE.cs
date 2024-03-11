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
        private bool ProcessWINDOW_UPDATEFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload)
        {
            /*
             * RFC 9113:
             * The WINDOW_UPDATE frame can be specific to a stream or to the entire connection. 
             * In the former case, the frame's stream identifier indicates the affected stream; 
             * in the latter, the value "0" indicates that the entire connection is the subject of the frame.
             */
            if (!Http2FrameReader.TryReadWINDOW_UPDATEFramePayload(ref payload, out var windowSizeIncrement))
            {
                logger.LogError("Error reading WINDOW_UPDATE payload");
                return false;
            }

            if (windowSizeIncrement == 0)
            {
                logger.LogError("Window Size Increment cannot be zero");
                return false;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                if (frame.StreamIdentifier == 0) // this is a connection-wise setting
                {
                    logger.LogDebug("Window Size Increment is set: {v}", windowSizeIncrement);

                    this.windowSizeIncrement = windowSizeIncrement;
                }
                else
                {
                    
                }
            }

            return true;
        }
    }
}
