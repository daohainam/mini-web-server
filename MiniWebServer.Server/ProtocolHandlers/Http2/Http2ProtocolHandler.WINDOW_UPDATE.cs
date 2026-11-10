using Microsoft.Extensions.Logging;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private void ProcessWINDOW_UPDATEFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.9: stream identifier 0 addresses the connection window, anything else a stream window
        if (!Http2FrameReader.TryReadWINDOW_UPDATEFramePayload(ref payload, out var increment))
        {
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "WINDOW_UPDATE payload must be 4 bytes");
        }

        if (frame.StreamIdentifier == 0)
        {
            if (increment == 0)
            {
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "Window size increment cannot be zero");
            }

            lock (flowControlLock)
            {
                connectionSendWindow += increment;
                if (connectionSendWindow > Http2ProtocolHandlerOptions.MaxWindowSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.FLOW_CONTROL_ERROR, "Connection send window exceeds the maximum window size");
                }

                SignalWindowUpdated();
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Connection send window increased by {inc} to {w}", increment, connectionSendWindow);
            }

            return;
        }

        if (increment == 0)
        {
            throw new Http2StreamException(frame.StreamIdentifier, Http2ErrorCode.PROTOCOL_ERROR, "Window size increment cannot be zero");
        }

        var stream = GetStream(frame.StreamIdentifier);
        if (stream == null)
        {
            if (IsIdleStream(frame.StreamIdentifier))
            {
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"WINDOW_UPDATE for idle stream {frame.StreamIdentifier}");
            }

            // RFC 9113 5.1: WINDOW_UPDATE may arrive for a short period after a stream is closed, it is ignored
            return;
        }

        lock (flowControlLock)
        {
            stream.SendWindow += increment;
            if (stream.SendWindow > Http2ProtocolHandlerOptions.MaxWindowSize)
            {
                throw new Http2StreamException(frame.StreamIdentifier, Http2ErrorCode.FLOW_CONTROL_ERROR, "Stream send window exceeds the maximum window size");
            }

            SignalWindowUpdated();
        }
    }
}
