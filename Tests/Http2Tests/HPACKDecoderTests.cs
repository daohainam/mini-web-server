using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace Http2Tests;

/// <summary>
/// Test vectors from RFC 7541 Appendix C, which exercise the static table, the dynamic table, Huffman coding and eviction.
/// </summary>
public class HPACKDecoderTests
{
    private static List<HPACKHeader> Decode(HPACKDecoder decoder, string hex)
    {
        var headers = new List<HPACKHeader>();
        decoder.Decode(hex.ToByteArray(), headers);
        return headers;
    }

    private static void AssertHeader(HPACKHeader header, string name, string value)
    {
        Assert.Equal(name, header.Name);
        Assert.Equal(value, header.Value);
    }

    [Fact]
    public void Decode_IntegerExamples_C1()
    {
        // C.1.1: 10 with a 5-bit prefix
        var bytes = new byte[] { 0b_0000_1010 };
        int offset = 0;
        Assert.Equal(10, HPACKInteger.Read(bytes, ref offset, 5));
        Assert.Equal(1, offset);

        // C.1.2: 1337 with a 5-bit prefix
        bytes = [0b_0001_1111, 0b_1001_1010, 0b_0000_1010];
        offset = 0;
        Assert.Equal(1337, HPACKInteger.Read(bytes, ref offset, 5));
        Assert.Equal(3, offset);

        // C.1.3: 42 at an octet boundary (8-bit prefix)
        bytes = [0b_0010_1010];
        offset = 0;
        Assert.Equal(42, HPACKInteger.Read(bytes, ref offset, 8));
    }

    [Fact]
    public void Decode_RequestsWithoutHuffman_C3_UsesDynamicTableAcrossBlocks()
    {
        var decoder = new HPACKDecoder();

        // C.3.1
        var headers = Decode(decoder, "828684410f7777772e6578616d706c652e636f6d");
        Assert.Equal(4, headers.Count);
        AssertHeader(headers[0], ":method", "GET");
        AssertHeader(headers[1], ":scheme", "http");
        AssertHeader(headers[2], ":path", "/");
        AssertHeader(headers[3], ":authority", "www.example.com");
        Assert.Equal(57, decoder.Table.Size);

        // C.3.2: :authority is now served from the dynamic table (index 62)
        headers = Decode(decoder, "828684be58086e6f2d6361636865");
        Assert.Equal(5, headers.Count);
        AssertHeader(headers[3], ":authority", "www.example.com");
        AssertHeader(headers[4], "cache-control", "no-cache");
        Assert.Equal(110, decoder.Table.Size);

        // C.3.3
        headers = Decode(decoder, "828785bf400a637573746f6d2d6b65790c637573746f6d2d76616c7565");
        Assert.Equal(5, headers.Count);
        AssertHeader(headers[0], ":method", "GET");
        AssertHeader(headers[1], ":scheme", "https");
        AssertHeader(headers[2], ":path", "/index.html");
        AssertHeader(headers[3], ":authority", "www.example.com");
        AssertHeader(headers[4], "custom-key", "custom-value");
        Assert.Equal(164, decoder.Table.Size);
        Assert.Equal(3, decoder.Table.Count);
    }

    [Fact]
    public void Decode_RequestsWithHuffman_C4()
    {
        var decoder = new HPACKDecoder();

        // C.4.1
        var headers = Decode(decoder, "828684418cf1e3c2e5f23a6ba0ab90f4ff");
        Assert.Equal(4, headers.Count);
        AssertHeader(headers[3], ":authority", "www.example.com");
        Assert.Equal(57, decoder.Table.Size);

        // C.4.2
        headers = Decode(decoder, "828684be5886a8eb10649cbf");
        Assert.Equal(5, headers.Count);
        AssertHeader(headers[4], "cache-control", "no-cache");

        // C.4.3
        headers = Decode(decoder, "828785bf408825a849e95ba97d7f8925a849e95bb8e8b4bf");
        Assert.Equal(5, headers.Count);
        AssertHeader(headers[4], "custom-key", "custom-value");
        Assert.Equal(164, decoder.Table.Size);
    }

    [Fact]
    public void Decode_ResponsesWithEviction_C5()
    {
        // the examples use a dynamic table limited to 256 octets so that entries get evicted
        var decoder = new HPACKDecoder(new HPACKHeaderTable(256));

        // C.5.1
        var headers = Decode(decoder, "4803333032580770726976617465611d4d6f6e2c203231204f637420323031332032303a31333a323120474d546e1768747470733a2f2f7777772e6578616d706c652e636f6d");
        Assert.Equal(4, headers.Count);
        AssertHeader(headers[0], ":status", "302");
        AssertHeader(headers[1], "cache-control", "private");
        AssertHeader(headers[2], "date", "Mon, 21 Oct 2013 20:13:21 GMT");
        AssertHeader(headers[3], "location", "https://www.example.com");
        Assert.Equal(222, decoder.Table.Size);

        // C.5.2: adding :status 307 evicts :status 302
        headers = Decode(decoder, "4803333037c1c0bf");
        Assert.Equal(4, headers.Count);
        AssertHeader(headers[0], ":status", "307");
        AssertHeader(headers[1], "cache-control", "private");
        AssertHeader(headers[2], "date", "Mon, 21 Oct 2013 20:13:21 GMT");
        AssertHeader(headers[3], "location", "https://www.example.com");
        Assert.Equal(222, decoder.Table.Size);
        Assert.Equal(4, decoder.Table.Count);

        // C.5.3
        headers = Decode(decoder, "88c1611d4d6f6e2c203231204f637420323031332032303a31333a323220474d54c05a04677a69707738666f6f3d4153444a4b48514b425a584f5157454f50495541585157454f49553b206d61782d6167653d333630303b2076657273696f6e3d31");
        Assert.Equal(6, headers.Count);
        AssertHeader(headers[0], ":status", "200");
        AssertHeader(headers[1], "cache-control", "private");
        AssertHeader(headers[2], "date", "Mon, 21 Oct 2013 20:13:22 GMT");
        AssertHeader(headers[3], "location", "https://www.example.com");
        AssertHeader(headers[4], "content-encoding", "gzip");
        AssertHeader(headers[5], "set-cookie", "foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1");
        Assert.Equal(215, decoder.Table.Size);
        Assert.Equal(3, decoder.Table.Count);
    }

    [Fact]
    public void Decode_NeverIndexedLiteral_IsMarkedSensitiveAndNotAddedToTable()
    {
        var decoder = new HPACKDecoder();

        // 0001 0000 (never indexed, new name) "password" "secret"
        var headers = Decode(decoder, "10" + "08" + "70617373776f7264" + "06" + "736563726574");

        Assert.Single(headers);
        AssertHeader(headers[0], "password", "secret");
        Assert.True(headers[0].Sensitive);
        Assert.Equal(0, decoder.Table.Count);
    }

    [Fact]
    public void Decode_DynamicTableSizeUpdate_AtStartOfBlock_IsApplied()
    {
        var decoder = new HPACKDecoder(new HPACKHeaderTable(4096));

        // 001 00000 => size 0, followed by an indexed :method GET
        var headers = Decode(decoder, "2082");

        Assert.Single(headers);
        Assert.Equal(0, decoder.Table.MaxSize);
    }

    [Fact]
    public void Decode_DynamicTableSizeUpdate_AboveLimit_Throws()
    {
        var decoder = new HPACKDecoder(new HPACKHeaderTable(4096));

        // 001 11111 + continuation => 4096 + 1
        Assert.Throws<HPACKDecodingException>(() => Decode(decoder, "3fe21f"));
    }

    [Fact]
    public void Decode_DynamicTableSizeUpdate_NotAtStart_Throws()
    {
        var decoder = new HPACKDecoder();

        Assert.Throws<HPACKDecodingException>(() => Decode(decoder, "8220"));
    }

    [Theory]
    [InlineData("80")] // index 0
    [InlineData("be")] // dynamic index on an empty table
    [InlineData("41")] // literal with missing value
    [InlineData("418f7777772e6578616d706c652e636f6d")] // string length exceeds block
    [InlineData("8cf1e3c2e5f23a6ba0ab90f4")] // truncated Huffman string
    public void Decode_MalformedBlock_Throws(string hex)
    {
        var decoder = new HPACKDecoder();

        Assert.Throws<HPACKDecodingException>(() => Decode(decoder, hex));
    }

    [Fact]
    public void Decode_HeaderListTooLarge_ThrowsButKeepsTableInSync()
    {
        var table = new HPACKHeaderTable();
        var decoder = new HPACKDecoder(table, maxHeaderListSize: 60, maxHeaderCount: 100);

        // C.3.1: 4 fields, list size = (32+7+3) + (32+7+4) + (32+5+1) + (32+10+15) = 180 > 60
        Assert.Throws<HPACKHeaderListTooLargeException>(() => Decode(decoder, "828684410f7777772e6578616d706c652e636f6d"));

        // the :authority entry was still inserted into the dynamic table
        Assert.Equal(1, table.Count);
        Assert.True(table.TryGetHeader(62, out var header));
        Assert.Equal(":authority", header!.Name);
    }

    [Fact]
    public void Encode_RoundTrip_ThroughDecoder()
    {
        var encoder = new HPACKEncoder();
        var decoder = new HPACKDecoder();
        var output = new ArrayBufferWriter<byte>();

        var headers = new List<HPACKHeader>
        {
            new(":status", "200"),
            new(":status", "418"),
            new("content-type", "text/html; charset=utf-8"),
            new("x-custom-header", "value"),
            new("authorization", "Bearer token", sensitive: true),
            new("x-long", new string('a', 300))
        };

        encoder.Encode(output, headers);
        var decoded = decoder.Decode(new ReadOnlySequence<byte>(output.WrittenMemory));

        Assert.Equal(headers.Count, decoded.Count);
        for (int i = 0; i < headers.Count; i++)
        {
            AssertHeader(decoded[i], headers[i].Name, headers[i].Value);
        }

        Assert.True(decoded[4].Sensitive);
        Assert.Equal(0, decoder.Table.Count); // the encoder never indexes
        Assert.Equal(0x88, output.WrittenSpan[0]); // :status 200 is static index 8
    }

    [Fact]
    public void Encode_AfterPeerLowersTableSize_EmitsSizeUpdateOnce()
    {
        var encoder = new HPACKEncoder();
        encoder.SetPeerMaxTableSize(0);

        var output = new ArrayBufferWriter<byte>();
        encoder.Encode(output, [new HPACKHeader(":status", "200")]);
        Assert.Equal(new byte[] { 0x20, 0x88 }, output.WrittenSpan.ToArray());

        output.Clear();
        encoder.Encode(output, [new HPACKHeader(":status", "200")]);
        Assert.Equal(new byte[] { 0x88 }, output.WrittenSpan.ToArray());

        // a decoder with a limit of 0 must accept the block
        var decoder = new HPACKDecoder(new HPACKHeaderTable(0));
        decoder.Decode([0x20, 0x88], []);
    }
}
