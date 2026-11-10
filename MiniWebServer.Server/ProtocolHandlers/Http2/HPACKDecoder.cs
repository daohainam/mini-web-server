using System.Buffers;
using System.Text;
using Hpack = global::Http2.Hpack;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Decodes HPACK header blocks (RFC 7541) into header fields, keeping the dynamic table in sync between blocks.
/// One decoder instance belongs to one HTTP/2 connection and must only be used from the connection's read loop.
/// </summary>
public sealed class HPACKDecoder
{
    public const int DefaultMaxHeaderListSize = 64 * 1024;
    public const int DefaultMaxHeaderCount = 100;

    private readonly HPACKHeaderTable table;

    public HPACKDecoder() : this(new HPACKHeaderTable())
    {
    }

    public HPACKDecoder(HPACKHeaderTable table, int maxHeaderListSize = DefaultMaxHeaderListSize, int maxHeaderCount = DefaultMaxHeaderCount)
    {
        this.table = table ?? throw new ArgumentNullException(nameof(table));
        MaxHeaderListSize = maxHeaderListSize;
        MaxHeaderCount = maxHeaderCount;
    }

    public HPACKHeaderTable Table => table;

    /// <summary>
    /// Maximum accepted size of a decoded header list, measured like SETTINGS_MAX_HEADER_LIST_SIZE (RFC 9113 6.5.2).
    /// </summary>
    public int MaxHeaderListSize { get; }

    public int MaxHeaderCount { get; }

    /// <summary>
    /// Decodes a complete header block (all HEADERS/CONTINUATION fragments concatenated) and appends the fields to <paramref name="headers"/>.
    /// Throws <see cref="HPACKDecodingException"/> on malformed input, which is a COMPRESSION_ERROR for the connection.
    /// </summary>
    public void Decode(ReadOnlySpan<byte> block, List<HPACKHeader> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        int offset = 0;
        bool sizeUpdateAllowed = true; // RFC 7541 4.2: size updates must appear at the beginning of a header block
        long listSize = 0;
        bool tooLarge = false;

        while (offset < block.Length)
        {
            byte b = block[offset];

            if ((b & 0b_1000_0000) != 0)
            {
                // 6.1 Indexed Header Field Representation
                int index = HPACKInteger.Read(block, ref offset, 7);
                AddHeader(headers, LookupHeader(index), ref listSize, ref tooLarge);
                sizeUpdateAllowed = false;
            }
            else if ((b & 0b_1100_0000) == 0b_0100_0000)
            {
                // 6.2.1 Literal Header Field with Incremental Indexing
                var header = ReadLiteral(block, ref offset, 6, sensitive: false);
                table.Add(header);
                AddHeader(headers, header, ref listSize, ref tooLarge);
                sizeUpdateAllowed = false;
            }
            else if ((b & 0b_1111_0000) == 0b_0000_0000)
            {
                // 6.2.2 Literal Header Field without Indexing
                AddHeader(headers, ReadLiteral(block, ref offset, 4, sensitive: false), ref listSize, ref tooLarge);
                sizeUpdateAllowed = false;
            }
            else if ((b & 0b_1111_0000) == 0b_0001_0000)
            {
                // 6.2.3 Literal Header Field Never Indexed
                AddHeader(headers, ReadLiteral(block, ref offset, 4, sensitive: true), ref listSize, ref tooLarge);
                sizeUpdateAllowed = false;
            }
            else
            {
                // 6.3 Dynamic Table Size Update (001xxxxx)
                if (!sizeUpdateAllowed)
                {
                    throw new HPACKDecodingException("Dynamic table size update is only allowed at the beginning of a header block");
                }

                int maxSize = HPACKInteger.Read(block, ref offset, 5);
                table.SetMaxSize(maxSize);
            }
        }

        if (tooLarge)
        {
            // the whole block was decoded so the dynamic table stays in sync, the caller decides what to do with the stream
            throw new HPACKHeaderListTooLargeException("Header list exceeds the configured limits");
        }
    }

    public List<HPACKHeader> Decode(ReadOnlySequence<byte> block)
    {
        var headers = new List<HPACKHeader>();
        Decode(block.IsSingleSegment ? block.FirstSpan : block.ToArray(), headers);

        return headers;
    }

    private HPACKHeader LookupHeader(int index)
    {
        if (index == 0 || !table.TryGetHeader(index, out var header))
        {
            throw new HPACKDecodingException($"Invalid header index {index}");
        }

        return header!;
    }

    private HPACKHeader ReadLiteral(ReadOnlySpan<byte> block, ref int offset, int prefixBits, bool sensitive)
    {
        int nameIndex = HPACKInteger.Read(block, ref offset, prefixBits);
        string name = nameIndex == 0 ? ReadString(block, ref offset) : LookupHeader(nameIndex).Name;
        string value = ReadString(block, ref offset);

        return new HPACKHeader(name, value, sensitive);
    }

    private static string ReadString(ReadOnlySpan<byte> block, ref int offset)
    {
        if (offset >= block.Length)
        {
            throw new HPACKDecodingException("Truncated string");
        }

        bool huffman = (block[offset] & 0b_1000_0000) != 0;
        int length = HPACKInteger.Read(block, ref offset, 7);
        if (length > block.Length - offset)
        {
            throw new HPACKDecodingException("String length exceeds header block");
        }

        var bytes = block.Slice(offset, length);
        offset += length;

        return huffman ? DecodeHuffman(bytes) : Encoding.Latin1.GetString(bytes);
    }

    private static string DecodeHuffman(ReadOnlySpan<byte> bytes)
    {
        try
        {
            return Hpack.Huffman.Decode(new ArraySegment<byte>(bytes.ToArray()), ArrayPool<byte>.Shared);
        }
        catch (Exception ex)
        {
            throw new HPACKDecodingException($"Invalid Huffman encoded string: {ex.Message}");
        }
    }

    private void AddHeader(List<HPACKHeader> headers, HPACKHeader header, ref long listSize, ref bool tooLarge)
    {
        listSize += header.Size;
        if (tooLarge || listSize > MaxHeaderListSize || headers.Count >= MaxHeaderCount)
        {
            tooLarge = true; // decoding continues so that every dynamic table insertion is still applied
            return;
        }

        headers.Add(header);
    }
}
