using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private bool ProcessPRIORITYFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload, out Http2FramePRIOTITYPayload priorityPayload)
    {
        if (!Http2FrameReader.TryReadPRIORITYFramePayload(ref payload, out priorityPayload) || priorityPayload == null)
        {
            logger.LogError("Error reading PRIORITY payload");
            return false;
        }

        if (frame.StreamIdentifier == 0) {
            logger.LogError("PRIORITY payload error: StreamIdentifier == 0");
            return false;
        }

        // since PRIORITY is deprecated so we do nothing

        return true;
    }
}
