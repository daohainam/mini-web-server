using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Serializes frames into caller supplied buffers. Every method returns the number of bytes written, frame header included.
/// </summary>
internal static class Http2FrameWriter
{
    public const int FrameHeaderLength = Http2FrameReader.HeaderLength;
    private const int PING_OPAQUE_DATA_SIZE = 8;

    public static void WriteFrameHeader(Span<byte> buffer, int payloadLength, Http2FrameType frameType, Http2FrameFlags flags, uint streamId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, FrameHeaderLength);
        ArgumentOutOfRangeException.ThrowIfNegative(payloadLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payloadLength, 0xFF_FFFF);

        buffer[0] = (byte)((payloadLength >> 16) & 0xFF);
        buffer[1] = (byte)((payloadLength >> 8) & 0xFF);
        buffer[2] = (byte)(payloadLength & 0xFF);
        buffer[3] = (byte)frameType;
        buffer[4] = (byte)flags;
        buffer[5] = (byte)((streamId >> 24) & 0x7F); // the reserved bit must stay unset
        buffer[6] = (byte)((streamId >> 16) & 0xFF);
        buffer[7] = (byte)((streamId >> 8) & 0xFF);
        buffer[8] = (byte)(streamId & 0xFF);
    }

    private static void EnsureCapacity(byte[] writePayload, int required)
    {
        ArgumentNullException.ThrowIfNull(writePayload);

        if (writePayload.Length < required)
        {
            throw new InternalHttp2Exception("Payload size too small");
        }
    }

    public static int SerializeSETTINGSFrame(Http2Frame frame, IEnumerable<Http2FrameSETTINGSItem> settingItems, byte[] writePayload)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(settingItems);

        var items = settingItems as IReadOnlyCollection<Http2FrameSETTINGSItem> ?? settingItems.ToArray();
        int length = items.Count * 6; // RFC 9113 6.5.1: each setting is a 16-bit identifier and a 32-bit value
        EnsureCapacity(writePayload, FrameHeaderLength + length);

        // SETTINGS frames always apply to the connection, stream identifier 0
        WriteFrameHeader(writePayload, length, Http2FrameType.SETTINGS, frame.Flags, 0);

        int idx = FrameHeaderLength;
        foreach (var settingItem in items)
        {
            writePayload[idx++] = (byte)(((ushort)settingItem.Identifier >> 8) & 0xFF);
            writePayload[idx++] = (byte)((ushort)settingItem.Identifier & 0xFF);

            writePayload[idx++] = (byte)((settingItem.Value >> 24) & 0xFF);
            writePayload[idx++] = (byte)((settingItem.Value >> 16) & 0xFF);
            writePayload[idx++] = (byte)((settingItem.Value >> 8) & 0xFF);
            writePayload[idx++] = (byte)(settingItem.Value & 0xFF);
        }

        return idx;
    }

    public static int SerializePINGFrame(Http2Frame frame, byte[] opaqueData, byte[] writePayload)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(opaqueData);

        if (opaqueData.Length != PING_OPAQUE_DATA_SIZE)
        {
            throw new InternalHttp2Exception("Invalid OpaqueData size");
        }

        EnsureCapacity(writePayload, FrameHeaderLength + PING_OPAQUE_DATA_SIZE);

        WriteFrameHeader(writePayload, PING_OPAQUE_DATA_SIZE, Http2FrameType.PING, frame.Flags, 0);
        Array.Copy(opaqueData, 0, writePayload, FrameHeaderLength, PING_OPAQUE_DATA_SIZE);

        return FrameHeaderLength + PING_OPAQUE_DATA_SIZE;
    }

    public static int SerializeGOAWAYFrame(uint lastStreamId, Http2ErrorCode errorCode, byte[] writePayload)
    {
        EnsureCapacity(writePayload, FrameHeaderLength + 8);

        WriteFrameHeader(writePayload, 8, Http2FrameType.GOAWAY, Http2FrameFlags.NONE, 0);
        WriteUInt32(writePayload.AsSpan(FrameHeaderLength), lastStreamId & 0x7FFF_FFFF);
        WriteUInt32(writePayload.AsSpan(FrameHeaderLength + 4), (uint)errorCode);

        return FrameHeaderLength + 8;
    }

    public static int SerializeRST_STREAMFrame(uint streamId, Http2ErrorCode errorCode, byte[] writePayload)
    {
        EnsureCapacity(writePayload, FrameHeaderLength + 4);

        WriteFrameHeader(writePayload, 4, Http2FrameType.RST_STREAM, Http2FrameFlags.NONE, streamId);
        WriteUInt32(writePayload.AsSpan(FrameHeaderLength), (uint)errorCode);

        return FrameHeaderLength + 4;
    }

    public static int SerializeWINDOW_UPDATEFrame(uint streamId, uint windowSizeIncrement, byte[] writePayload)
    {
        ArgumentOutOfRangeException.ThrowIfZero(windowSizeIncrement);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(windowSizeIncrement, Http2ProtocolHandlerOptions.MaxWindowSize);
        EnsureCapacity(writePayload, FrameHeaderLength + 4);

        WriteFrameHeader(writePayload, 4, Http2FrameType.WINDOW_UPDATE, Http2FrameFlags.NONE, streamId);
        WriteUInt32(writePayload.AsSpan(FrameHeaderLength), windowSizeIncrement);

        return FrameHeaderLength + 4;
    }

    public static int SerializeHEADERSFrame(uint streamId, ReadOnlySpan<byte> fieldBlockFragment, bool endHeaders, bool endStream, byte[] writePayload)
    {
        EnsureCapacity(writePayload, FrameHeaderLength + fieldBlockFragment.Length);

        var flags = Http2FrameFlags.NONE;
        if (endHeaders)
        {
            flags |= Http2FrameFlags.END_HEADERS;
        }

        if (endStream)
        {
            flags |= Http2FrameFlags.END_STREAM;
        }

        WriteFrameHeader(writePayload, fieldBlockFragment.Length, Http2FrameType.HEADERS, flags, streamId);
        fieldBlockFragment.CopyTo(writePayload.AsSpan(FrameHeaderLength));

        return FrameHeaderLength + fieldBlockFragment.Length;
    }

    public static int SerializeCONTINUATIONFrame(uint streamId, ReadOnlySpan<byte> fieldBlockFragment, bool endHeaders, byte[] writePayload)
    {
        EnsureCapacity(writePayload, FrameHeaderLength + fieldBlockFragment.Length);

        var flags = endHeaders ? Http2FrameFlags.END_HEADERS : Http2FrameFlags.NONE;

        WriteFrameHeader(writePayload, fieldBlockFragment.Length, Http2FrameType.CONTINUATION, flags, streamId);
        fieldBlockFragment.CopyTo(writePayload.AsSpan(FrameHeaderLength));

        return FrameHeaderLength + fieldBlockFragment.Length;
    }

    /// <summary>
    /// Writes a complete header block as one HEADERS frame followed by as many CONTINUATION frames as needed (RFC 9113 4.3).
    /// </summary>
    public static void WriteHeaderBlock(IBufferWriter<byte> output, uint streamId, ReadOnlySpan<byte> headerBlock, bool endStream, int maxFrameSize)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxFrameSize, 0);

        int offset = 0;
        bool first = true;
        do
        {
            int chunk = Math.Min(maxFrameSize, headerBlock.Length - offset);
            bool last = offset + chunk >= headerBlock.Length;
            var flags = last ? Http2FrameFlags.END_HEADERS : Http2FrameFlags.NONE;
            var type = Http2FrameType.CONTINUATION;
            if (first)
            {
                type = Http2FrameType.HEADERS;
                if (endStream)
                {
                    flags |= Http2FrameFlags.END_STREAM;
                }
            }

            var span = output.GetSpan(FrameHeaderLength + chunk);
            WriteFrameHeader(span, chunk, type, flags, streamId);
            headerBlock.Slice(offset, chunk).CopyTo(span[FrameHeaderLength..]);
            output.Advance(FrameHeaderLength + chunk);

            offset += chunk;
            first = false;
        } while (offset < headerBlock.Length);
    }

    public static int SerializeDATAFrame(uint streamId, byte[] data, int offset, int length, bool endStream, byte[] writePayload)
    {
        ArgumentNullException.ThrowIfNull(data);

        return SerializeDATAFrame(streamId, data.AsSpan(offset, length), endStream, writePayload);
    }

    public static int SerializeDATAFrame(uint streamId, ReadOnlySpan<byte> data, bool endStream, byte[] writePayload)
    {
        EnsureCapacity(writePayload, FrameHeaderLength + data.Length);

        var flags = endStream ? Http2FrameFlags.END_STREAM : Http2FrameFlags.NONE;

        WriteFrameHeader(writePayload, data.Length, Http2FrameType.DATA, flags, streamId);
        data.CopyTo(writePayload.AsSpan(FrameHeaderLength));

        return FrameHeaderLength + data.Length;
    }

    private static void WriteUInt32(Span<byte> buffer, uint value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }
}
