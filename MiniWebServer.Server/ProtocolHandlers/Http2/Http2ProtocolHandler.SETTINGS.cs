using Microsoft.Extensions.Logging;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private async Task ProcessSETTINGSFrameAsync(Http2Frame frame, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
    {
        // RFC 9113 6.5
        if (frame.StreamIdentifier != 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "SETTINGS frames must use stream identifier 0");
        }

        if (frame.Flags.HasFlag(Http2FrameFlags.ACK))
        {
            if (payload.Length != 0)
            {
                throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "A SETTINGS frame with the ACK flag must have an empty payload");
            }

            logger.LogDebug("Received SETTINGS ACK");
            return;
        }

        if (!Http2FrameReader.TryReadSETTINGSFramePayload(ref payload, out var settings))
        {
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, "SETTINGS payload length must be a multiple of 6");
        }

        foreach (var setting in settings)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Peer setting {id} = {v}", setting.Identifier, setting.Value);
            }

            ApplyPeerSetting(setting.Identifier, setting.Value);
        }

        // RFC 9113 6.5.3: the receiver must acknowledge every SETTINGS frame it applied
        var ackFrame = new Http2Frame { FrameType = Http2FrameType.SETTINGS, Flags = Http2FrameFlags.ACK };
        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength);
        try
        {
            int length = Http2FrameWriter.SerializeSETTINGSFrame(ackFrame, [], buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void ApplyPeerSetting(Http2FrameSettings identifier, uint value)
    {
        // RFC 9113 6.5.2, every one of these describes what the peer is willing to receive
        switch (identifier)
        {
            case Http2FrameSettings.SETTINGS_HEADER_TABLE_SIZE:
                // the size of the dynamic table our encoder may use, any value is valid
                encoder.SetPeerMaxTableSize((int)Math.Min(value, int.MaxValue));
                break;

            case Http2FrameSettings.SETTINGS_ENABLE_PUSH:
                if (value > 1)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"Invalid SETTINGS_ENABLE_PUSH value {value}");
                }
                break;

            case Http2FrameSettings.SETTINGS_MAX_CONCURRENT_STREAMS:
                peerMaxConcurrentStreams = value;
                break;

            case Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE:
                if (value > Http2ProtocolHandlerOptions.MaxWindowSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.FLOW_CONTROL_ERROR, $"Invalid SETTINGS_INITIAL_WINDOW_SIZE value {value}");
                }

                UpdatePeerInitialWindowSize(value);
                break;

            case Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE:
                if (value < Http2ProtocolHandlerOptions.MinMaxFrameSize || value > Http2ProtocolHandlerOptions.MaxMaxFrameSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"Invalid SETTINGS_MAX_FRAME_SIZE value {value}");
                }

                peerMaxFrameSize = value;
                break;

            case Http2FrameSettings.SETTINGS_MAX_HEADER_LIST_SIZE:
                peerMaxHeaderListSize = value;
                break;

            default:
                // unknown or unsupported settings must be ignored
                break;
        }
    }

    private void UpdatePeerInitialWindowSize(uint value)
    {
        // RFC 9113 6.9.2: the change applies to the send window of every stream, open or half-closed
        lock (flowControlLock)
        {
            long delta = (long)value - peerInitialWindowSize;
            peerInitialWindowSize = value;

            foreach (var stream in streams.Values)
            {
                stream.SendWindow += delta;
                if (stream.SendWindow > Http2ProtocolHandlerOptions.MaxWindowSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.FLOW_CONTROL_ERROR, $"Send window of stream {stream.StreamId} exceeds the maximum window size");
                }
            }

            if (delta > 0)
            {
                SignalWindowUpdated();
            }
        }
    }
}
