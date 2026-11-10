using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Parses frame headers and the fixed-layout frame payloads of RFC 9113 section 6.
/// </summary>
public static class Http2FrameReader
{
    public const int HeaderLength = 9; // RFC 9113 4.1: all frames begin with a fixed 9-octet header

    /// <summary>
    /// Reads one complete frame from <paramref name="buffer"/>. Returns false when the buffer does not hold a complete frame yet.
    /// Throws an <see cref="Http2ConnectionException"/> (FRAME_SIZE_ERROR) when the payload exceeds <paramref name="maxFrameSize"/>.
    /// </summary>
    public static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, ref Http2Frame frame, uint maxFrameSize, out ReadOnlySequence<byte> payload)
    {
        ArgumentNullException.ThrowIfNull(frame);

        payload = ReadOnlySequence<byte>.Empty;
        if (buffer.Length < HeaderLength)
        {
            return false;
        }

        var headerSlice = buffer.Slice(0, HeaderLength);
        var header = headerSlice.IsSingleSegment ? headerSlice.FirstSpan : headerSlice.ToArray();
        var payloadLength = ReadPayloadLength(header);

        if (payloadLength > maxFrameSize)
        {
            // RFC 9113 4.2: a frame exceeding SETTINGS_MAX_FRAME_SIZE is a connection error
            throw new Http2ConnectionException(Http2ErrorCode.FRAME_SIZE_ERROR, $"Frame payload of {payloadLength} bytes exceeds the maximum frame size of {maxFrameSize}");
        }

        if (buffer.Length < HeaderLength + payloadLength)
        {
            return false;
        }

        frame.Length = payloadLength;
        frame.FrameType = GetFrameType(header[3]); // RFC 9113 4.1: unknown types are ignored by the caller, never rejected here
        frame.Flags = (Http2FrameFlags)header[4];
        frame.StreamIdentifier = GetStreamIdentifier(header);

        payload = buffer.Slice(HeaderLength, payloadLength);
        buffer = buffer.Slice(payload.End);

        return true;
    }

    internal static uint GetStreamIdentifier(ReadOnlySpan<byte> header)
    {
        // the reserved first bit must be ignored when receiving
        return (uint)((header[5] & 0b_0111_1111) << 24 | header[6] << 16 | header[7] << 8 | header[8]);
    }

    internal static Http2FrameType GetFrameType(byte b) => (Http2FrameType)b;

    public static int ReadPayloadLength(ReadOnlySpan<byte> header)
    {
        return header[0] << 16 | header[1] << 8 | header[2];
    }

    private static ReadOnlySpan<byte> AsSpan(ReadOnlySequence<byte> sequence)
    {
        return sequence.IsSingleSegment ? sequence.FirstSpan : sequence.ToArray();
    }

    private static uint ReadUInt31(ReadOnlySpan<byte> bytes)
    {
        return (uint)((bytes[0] & 0b_0111_1111) << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> bytes)
    {
        return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
    }

    public static bool TryReadSETTINGSFramePayload(ref ReadOnlySequence<byte> payload, out Http2FrameSETTINGSItem[] settings)
    {
        if (payload.Length % 6 != 0)
        {
            settings = [];
            return false;
        }

        var payloadBytes = AsSpan(payload);

        int count = payloadBytes.Length / 6;
        settings = new Http2FrameSETTINGSItem[count];
        for (int i = 0; i < count; i++)
        {
            int si = i * 6;
            settings[i] = new Http2FrameSETTINGSItem()
            {
                Identifier = (Http2FrameSettings)(payloadBytes[si] << 8 | payloadBytes[si + 1]),
                Value = ReadUInt32(payloadBytes[(si + 2)..])
            };
        }

        return true;
    }

    public static bool TryReadWINDOW_UPDATEFramePayload(ref ReadOnlySequence<byte> payload, out uint windowSizeIncrement)
    {
        if (payload.Length != 4)
        {
            windowSizeIncrement = 0;
            return false;
        }

        windowSizeIncrement = ReadUInt31(AsSpan(payload));
        return true;
    }

    public static bool TryReadPINGFramePayload(ref ReadOnlySequence<byte> payload, out byte[] opaqueData)
    {
        if (payload.Length != 8) // RFC 9113 6.7: any other length is a connection error of type FRAME_SIZE_ERROR
        {
            opaqueData = [];
            return false;
        }

        opaqueData = new byte[8];
        payload.CopyTo(opaqueData);

        return true;
    }

    public static bool TryReadPRIORITYFramePayload(ref ReadOnlySequence<byte> payload, out Http2FramePRIORITYPayload priorityPayload)
    {
        if (payload.Length != 5)
        {
            priorityPayload = Http2FramePRIORITYPayload.Empty;
            return false;
        }

        var payloadBytes = AsSpan(payload);
        priorityPayload = new()
        {
            Exclusive = (payloadBytes[0] & 0b_1000_0000) != 0,
            StreamDependency = ReadUInt31(payloadBytes),
            Weight = payloadBytes[4]
        };

        return true;
    }

    public static bool TryReadRST_STREAMFramePayload(ref ReadOnlySequence<byte> payload, out Http2ErrorCode errorCode)
    {
        if (payload.Length != 4)
        {
            errorCode = Http2ErrorCode.NO_ERROR;
            return false;
        }

        errorCode = (Http2ErrorCode)ReadUInt32(AsSpan(payload));
        return true;
    }

    public static bool TryReadGOAWAYFramePayload(ref ReadOnlySequence<byte> payload, out uint lastStreamId, out Http2ErrorCode errorCode, out ReadOnlySequence<byte> debugData)
    {
        if (payload.Length < 8)
        {
            lastStreamId = 0;
            errorCode = Http2ErrorCode.NO_ERROR;
            debugData = ReadOnlySequence<byte>.Empty;
            return false;
        }

        var head = AsSpan(payload.Slice(0, 8));
        lastStreamId = ReadUInt31(head);
        errorCode = (Http2ErrorCode)ReadUInt32(head[4..]);
        debugData = payload.Slice(8);

        return true;
    }

    /// <summary>
    /// Strips the padding of a DATA frame (RFC 9113 6.1). Returns false when the pad length is not smaller than the payload.
    /// </summary>
    public static bool TryReadDATAFramePayload(Http2Frame frame, ReadOnlySequence<byte> payload, out ReadOnlySequence<byte> data)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (!frame.Flags.HasFlag(Http2FrameFlags.PADDED))
        {
            data = payload;
            return true;
        }

        if (payload.IsEmpty)
        {
            data = ReadOnlySequence<byte>.Empty;
            return false;
        }

        int padLength = payload.FirstSpan[0];
        if (padLength >= payload.Length)
        {
            data = ReadOnlySequence<byte>.Empty;
            return false;
        }

        data = payload.Slice(1, payload.Length - 1 - padLength);
        return true;
    }

    /// <summary>
    /// Splits a HEADERS frame payload (RFC 9113 6.2) into its pad length, priority fields and header block fragment.
    /// Returns false when the padding or priority fields do not fit into the payload.
    /// </summary>
    public static bool TryReadHEADERSFramePayload(Http2Frame frame, ReadOnlySequence<byte> payload, out Http2FrameHEADERSPayload headersPayload)
    {
        ArgumentNullException.ThrowIfNull(frame);

        headersPayload = new();

        bool padded = frame.Flags.HasFlag(Http2FrameFlags.PADDED);
        bool hasPriority = frame.Flags.HasFlag(Http2FrameFlags.PRIORITY);

        int prefixLength = (padded ? 1 : 0) + (hasPriority ? 5 : 0);
        if (payload.Length < prefixLength)
        {
            return false;
        }

        var prefix = AsSpan(payload.Slice(0, prefixLength));

        byte padLength = 0;
        int idx = 0;
        if (padded)
        {
            padLength = prefix[idx++];
            if (padLength > payload.Length - prefixLength)
            {
                return false;
            }
        }

        bool exclusive = false;
        uint streamDependency = 0;
        byte weight = 0;
        if (hasPriority)
        {
            exclusive = (prefix[idx] & 0b_1000_0000) != 0;
            streamDependency = ReadUInt31(prefix[idx..]);
            weight = prefix[idx + 4];
        }

        headersPayload = new()
        {
            PadLength = padLength,
            HasPriority = hasPriority,
            Exclusive = exclusive,
            StreamDependency = streamDependency,
            Weight = weight,
            FieldBlockFragment = payload.Slice(prefixLength, payload.Length - prefixLength - padLength)
        };

        return true;
    }
}
