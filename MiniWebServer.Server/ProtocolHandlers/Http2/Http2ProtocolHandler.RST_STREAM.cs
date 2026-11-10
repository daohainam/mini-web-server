using Microsoft.Extensions.Logging;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private void ProcessRST_STREAMFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.4
        if (frame.StreamIdentifier == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "RST_STREAM frames must not use stream identifier 0");
        }

        if (!Http2FrameReader.TryReadRST_STREAMFramePayload(ref payload, out var errorCode))
        {
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "RST_STREAM payload must be 4 bytes");
        }

        var stream = GetStream(frame.StreamIdentifier);
        if (stream == null)
        {
            if (IsIdleStream(frame.StreamIdentifier))
            {
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"RST_STREAM for idle stream {frame.StreamIdentifier}");
            }

            return; // already closed
        }

        logger.LogDebug("Stream {sid} reset by peer: {code}", frame.StreamIdentifier, errorCode);

        // the peer abandoned the stream: pending writers give up, the body reader gets an error, nothing more is sent on it
        CloseStream(stream, errorCode);
    }

    private void ProcessGOAWAYFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.8
        if (frame.StreamIdentifier != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "GOAWAY frames must use stream identifier 0");
        }

        if (!Http2FrameReader.TryReadGOAWAYFramePayload(ref payload, out var lastStreamId, out var errorCode, out _))
        {
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "GOAWAY payload must be at least 8 bytes");
        }

        if (errorCode == Http2ErrorCode.NO_ERROR)
        {
            logger.LogDebug("Peer sent GOAWAY, last stream {sid}", lastStreamId);
        }
        else
        {
            logger.LogWarning("Peer sent GOAWAY with error {code}, last stream {sid}", errorCode, lastStreamId);
        }

        // no new streams are accepted, the ones in flight finish and then ReadRequestAsync returns false
        goAwayReceived = true;
    }
}
