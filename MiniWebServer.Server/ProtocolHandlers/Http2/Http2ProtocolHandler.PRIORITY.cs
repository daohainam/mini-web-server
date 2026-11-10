using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private void ProcessPRIORITYFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.3: the priority scheme is deprecated, frames are validated and otherwise ignored
        if (frame.StreamIdentifier == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "PRIORITY frames must not use stream identifier 0");
        }

        if (!Http2FrameReader.TryReadPRIORITYFramePayload(ref payload, out var priority))
        {
            throw new Http2StreamException(frame.StreamIdentifier, Http2ErrorCode.FRAME_SIZE_ERROR, "PRIORITY payload must be 5 bytes");
        }

        if (priority.StreamDependency == frame.StreamIdentifier)
        {
            // RFC 9113 5.3.1 (via RFC 7540): a stream cannot depend on itself
            throw new Http2StreamException(frame.StreamIdentifier, Http2ErrorCode.PROTOCOL_ERROR, "A stream cannot depend on itself");
        }

        // PRIORITY is allowed in every stream state, including idle and closed, and never creates a stream
    }
}
