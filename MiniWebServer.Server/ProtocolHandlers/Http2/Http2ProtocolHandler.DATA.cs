using Microsoft.Extensions.Logging;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private async Task ProcessDATAFrameAsync(Http2Frame frame, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
    {
        // RFC 9113 6.1
        uint streamId = frame.StreamIdentifier;
        if (streamId == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "DATA frames must not use stream identifier 0");
        }

        if (!Http2FrameReader.TryReadDATAFramePayload(frame, payload, out var data))
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "Invalid padding in DATA frame");
        }

        var stream = GetStream(streamId);

        // RFC 9113 6.9.1: the whole payload, padding included, is charged against both receive windows
        int frameLength = (int)payload.Length;
        Http2StreamException? streamError = null;
        lock (flowControlLock)
        {
            if (frameLength > connectionReceiveWindow)
            {
                throw new Http2ConnectionException(Http2ErrorCode.FLOW_CONTROL_ERROR, "Connection receive window exceeded");
            }

            connectionReceiveWindow -= frameLength;
            unacknowledgedConnectionBytes += frameLength;

            if (stream != null)
            {
                if (frameLength > stream.ReceiveWindow)
                {
                    streamError = new Http2StreamException(streamId, Http2ErrorCode.FLOW_CONTROL_ERROR, "Stream receive window exceeded");
                }
                else
                {
                    stream.ReceiveWindow -= frameLength;
                    stream.UnacknowledgedReceivedBytes += frameLength;
                }
            }
        }

        // keep the connection window open even when the stream below is rejected
        await ReplenishConnectionWindowAsync(cancellationToken);

        if (stream == null)
        {
            if (IsIdleStream(streamId))
            {
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"DATA frame for idle stream {streamId}");
            }

            // RFC 9113 5.1: data for a stream we already closed or reset is discarded
            return;
        }

        if (streamError != null)
        {
            throw streamError;
        }

        if (stream.State == Http2StreamStates.CLOSED || stream.IsReset)
        {
            // RFC 9113 5.1: after we reset a stream (for example RST_STREAM NO_ERROR after a complete response) frames still in flight are ignored
            return;
        }

        if (stream.RemoteClosed)
        {
            throw new Http2StreamException(streamId, Http2ErrorCode.STREAM_CLOSED, "DATA frame after END_STREAM");
        }

        stream.ReceivedBodyBytes += data.Length;

        if (stream.ReceivedBodyBytes > config.MaxRequestBodySize)
        {
            throw new Http2StreamException(streamId, Http2ErrorCode.CANCEL, "Request body exceeds the maximum request body size");
        }

        if (stream.DeclaredContentLength is long declared && stream.ReceivedBodyBytes > declared)
        {
            throw new Http2StreamException(streamId, Http2ErrorCode.PROTOCOL_ERROR, "Request body is longer than content-length");
        }

        if (!data.IsEmpty)
        {
            foreach (var segment in data)
            {
                stream.Body.Writer.Write(segment.Span);
            }

            await stream.Body.Writer.FlushAsync(cancellationToken);
        }

        if (frame.Flags.HasFlag(Http2FrameFlags.END_STREAM))
        {
            CompleteRequestBody(stream);
        }
        else
        {
            await ReplenishStreamWindowAsync(stream, cancellationToken);
        }
    }

    private async Task ReplenishConnectionWindowAsync(CancellationToken cancellationToken)
    {
        // data is handed to the request body pipe right away, so the window is given back as soon as half of it is used
        uint increment = 0;
        lock (flowControlLock)
        {
            if (unacknowledgedConnectionBytes >= Http2ProtocolHandlerOptions.DefaultInitialWindowSize / 2)
            {
                increment = (uint)unacknowledgedConnectionBytes;
                unacknowledgedConnectionBytes = 0;
                connectionReceiveWindow += increment;
            }
        }

        if (increment > 0)
        {
            await SendWindowUpdateAsync(0, increment, cancellationToken);
        }
    }

    private async Task ReplenishStreamWindowAsync(Http2Stream stream, CancellationToken cancellationToken)
    {
        uint increment = 0;
        lock (flowControlLock)
        {
            if (!stream.RemoteClosed && stream.UnacknowledgedReceivedBytes >= options.InitialWindowSize / 2)
            {
                increment = (uint)stream.UnacknowledgedReceivedBytes;
                stream.UnacknowledgedReceivedBytes = 0;
                stream.ReceiveWindow += increment;
            }
        }

        if (increment > 0)
        {
            await SendWindowUpdateAsync(stream.StreamId, increment, cancellationToken);
        }
    }
}
