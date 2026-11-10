using Microsoft.Extensions.Logging;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private async Task ProcessPINGFrameAsync(Http2Frame frame, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
    {
        // RFC 9113 6.7
        if (frame.StreamIdentifier != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "PING frames must use stream identifier 0");
        }

        if (!Http2FrameReader.TryReadPINGFramePayload(ref payload, out var opaqueData))
        {
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "PING payload must be 8 bytes");
        }

        if (frame.Flags.HasFlag(Http2FrameFlags.ACK))
        {
            logger.LogDebug("Received PING ACK");
            return;
        }

        // reply with the same opaque data and the ACK flag set
        var pingFrame = new Http2Frame { FrameType = Http2FrameType.PING, Flags = Http2FrameFlags.ACK, Length = 8 };
        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + 8);
        try
        {
            int length = Http2FrameWriter.SerializePINGFrame(pingFrame, opaqueData, buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
