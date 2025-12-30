using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private bool ProcessPINGFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload)
    {
        if (!Http2FrameReader.TryReadPINGFramePayload(ref payload, out var opaqueData))
        {
            logger.LogError("Error reading PING payload");
            return false;
        }

        if (frame.StreamIdentifier != 0) {
            logger.LogError("PING payload error: StreamIdentifier != 0");
            return false;
        }

        if (frame.Flags.HasFlag(Http2FrameFlags.ACK))
        {
            logger.LogDebug("Received a PING_ACK frame");
        }
        else
        {
            // send back a PING with ACK turned off
            logger.LogDebug("Sending back a PING_ACK frame");
            var pingFrame = new Http2Frame()
            {
                FrameType = Http2FrameType.PING,
                Flags = Http2FrameFlags.ACK, 
                Length = 8
            };

            var writePayload = ArrayPool<byte>.Shared.Rent((int)maxFrameSize);
            int length = Http2FrameWriter.SerializePINGFrame(pingFrame, opaqueData, writePayload);
            if (length > 0)
            {
                protocolHandlerContext.Stream.Write(writePayload, 0, length);
            }

            ArrayPool<byte>.Shared.Return(writePayload);
        }

        return true;
    }
}
