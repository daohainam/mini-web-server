using System.Buffers;
using System.Text;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Encodes header lists into HPACK header blocks (RFC 7541). The encoder never adds entries to the dynamic table, so the
/// only dynamic table state it has to track is the maximum size the peer allows (SETTINGS_HEADER_TABLE_SIZE).
/// One encoder instance belongs to one HTTP/2 connection.
/// </summary>
public sealed class HPACKEncoder
{
    private int currentTableSize = HPACKHeaderTable.DefaultMaxSize;
    private bool sizeUpdatePending;

    /// <summary>
    /// Called when the peer changes SETTINGS_HEADER_TABLE_SIZE. A smaller value must be acknowledged with a dynamic table
    /// size update at the start of the next header block (RFC 7541 4.2).
    /// </summary>
    public void SetPeerMaxTableSize(int maxTableSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxTableSize);

        if (maxTableSize < currentTableSize)
        {
            currentTableSize = maxTableSize;
            sizeUpdatePending = true;
        }
    }

    public void Encode(IBufferWriter<byte> output, IEnumerable<HPACKHeader> headers)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(headers);

        if (sizeUpdatePending)
        {
            var span = output.GetSpan(HPACKInteger.GetEncodedLength(currentTableSize, 5));
            output.Advance(HPACKInteger.Write(span, currentTableSize, 5, 0b_0010_0000));
            sizeUpdatePending = false;
        }

        foreach (var header in headers)
        {
            EncodeHeader(output, header);
        }
    }

    public static void EncodeHeader(IBufferWriter<byte> output, HPACKHeader header)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(header);

        int index = HPACKStaticTable.GetIndex(header.Name, header.Value);
        if (index > 0 && !header.Sensitive)
        {
            // 6.1 Indexed Header Field Representation
            var span = output.GetSpan(HPACKInteger.GetEncodedLength(index, 7));
            output.Advance(HPACKInteger.Write(span, index, 7, 0b_1000_0000));
            return;
        }

        // 6.2.2 Literal Header Field without Indexing / 6.2.3 Never Indexed, both use a 4-bit prefix
        byte pattern = header.Sensitive ? (byte)0b_0001_0000 : (byte)0b_0000_0000;
        int nameIndex = HPACKStaticTable.GetNameIndex(header.Name);

        int nameLength = nameIndex > 0 ? 0 : Encoding.Latin1.GetByteCount(header.Name);
        int valueLength = Encoding.Latin1.GetByteCount(header.Value);
        int required = HPACKInteger.GetEncodedLength(nameIndex, 4)
            + (nameIndex > 0 ? 0 : HPACKInteger.GetEncodedLength(nameLength, 7) + nameLength)
            + HPACKInteger.GetEncodedLength(valueLength, 7) + valueLength;

        var buffer = output.GetSpan(required);
        int written = HPACKInteger.Write(buffer, nameIndex, 4, pattern);
        if (nameIndex == 0)
        {
            written += WriteString(buffer[written..], header.Name, nameLength);
        }

        written += WriteString(buffer[written..], header.Value, valueLength);

        output.Advance(written);
    }

    private static int WriteString(Span<byte> buffer, string value, int byteLength)
    {
        // strings are sent raw (H bit = 0), Huffman coding is optional for an encoder
        int written = HPACKInteger.Write(buffer, byteLength, 7, 0);
        written += Encoding.Latin1.GetBytes(value, buffer[written..]);

        return written;
    }
}
