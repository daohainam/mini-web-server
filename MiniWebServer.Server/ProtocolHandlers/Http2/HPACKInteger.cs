using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// HPACK integer representation, RFC 7541 5.1.
/// </summary>
public static class HPACKInteger
{
    /// <summary>
    /// Decodes an integer with an n-bit prefix starting at <paramref name="offset"/>, the bits above the prefix are ignored.
    /// </summary>
    public static int Read(ReadOnlySpan<byte> buffer, ref int offset, int prefixBits)
    {
        if (prefixBits < 1 || prefixBits > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(prefixBits));
        }

        if (offset >= buffer.Length)
        {
            throw new HPACKDecodingException("Truncated integer");
        }

        int prefixMax = (1 << prefixBits) - 1;
        int value = buffer[offset++] & prefixMax;
        if (value < prefixMax)
        {
            return value;
        }

        int shift = 0;
        byte next;
        do
        {
            if (offset >= buffer.Length)
            {
                throw new HPACKDecodingException("Truncated integer");
            }

            if (shift > 28)
            {
                throw new HPACKDecodingException("Integer too large");
            }

            next = buffer[offset++];
            long sum = value + ((long)(next & 0x7F) << shift);
            if (sum > int.MaxValue)
            {
                throw new HPACKDecodingException("Integer too large");
            }

            value = (int)sum;
            shift += 7;
        } while ((next & 0x80) != 0);

        return value;
    }

    public static int ReadInt(ref ReadOnlySequence<byte> payload, int prefixBits)
    {
        var buffer = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray();
        int offset = 0;
        int value = Read(buffer, ref offset, prefixBits);
        payload = payload.Slice(offset);

        return value;
    }

    /// <summary>
    /// Returns the number of bytes needed to encode <paramref name="value"/> with an n-bit prefix.
    /// </summary>
    public static int GetEncodedLength(int value, int prefixBits)
    {
        int prefixMax = (1 << prefixBits) - 1;
        if (value < prefixMax)
        {
            return 1;
        }

        int length = 1;
        value -= prefixMax;
        while (value >= 128)
        {
            value >>= 7;
            length++;
        }

        return length + 1;
    }

    /// <summary>
    /// Encodes <paramref name="value"/> with an n-bit prefix, OR-ing <paramref name="prefixPattern"/> into the first byte.
    /// </summary>
    public static int Write(Span<byte> buffer, int value, int prefixBits, byte prefixPattern)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        int prefixMax = (1 << prefixBits) - 1;
        if (value < prefixMax)
        {
            buffer[0] = (byte)(prefixPattern | value);
            return 1;
        }

        buffer[0] = (byte)(prefixPattern | prefixMax);
        int length = 1;
        value -= prefixMax;
        while (value >= 128)
        {
            buffer[length++] = (byte)(0x80 | (value & 0x7F));
            value >>= 7;
        }

        buffer[length++] = (byte)value;

        return length;
    }

    public static void WriteInt(int value, byte[] bytes, int prefixBits, out int length)
    {
        length = Write(bytes, value, prefixBits, 0);
    }
}
