using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace MiniWebServer.Server.Tests.ProtocolHandlers.Http2;

[TestClass()]
public class Http2FrameReaderTests
{
    [TestMethod()]
    public void TryReadHEADERSFramePayload_ValidPayload_ReturnsTrueAndHeadersPayload()
    {
        // Arrange
        var frame = new Http2Frame { FrameType = Http2FrameType.HEADERS, Flags = Http2FrameFlags.END_HEADERS };

        // from https://httpwg.org/specs/rfc7541.html#n-first-request_2
        var payload = new ReadOnlySequence<byte>([0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff]);
        var decoder = new HPACKDecoder();

        // Act
        var result = Http2FrameReader.TryReadHEADERSFramePayload(frame, payload, out var headersPayload);
        var headers = decoder.Decode(headersPayload.FieldBlockFragment);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(headersPayload);
        Assert.AreEqual(4, headers.Count);
        Assert.AreEqual(":method", headers[0].Name);
        Assert.AreEqual("GET", headers[0].Value);
        Assert.AreEqual(":scheme", headers[1].Name);
        Assert.AreEqual("http", headers[1].Value);
        Assert.AreEqual(":path", headers[2].Name);
        Assert.AreEqual("/", headers[2].Value);
        Assert.AreEqual(":authority", headers[3].Name);
        Assert.AreEqual("www.example.com", headers[3].Value);
    }

    [TestMethod()]
    public void TryReadHEADERSFramePayload_InvalidHeaderBlock_DecoderThrows()
    {
        // Arrange: an indexed field with index 0 is always invalid
        var frame = new Http2Frame { FrameType = Http2FrameType.HEADERS, Flags = Http2FrameFlags.END_HEADERS };
        var payload = new ReadOnlySequence<byte>([0x80]);
        var decoder = new HPACKDecoder();

        // Act
        var result = Http2FrameReader.TryReadHEADERSFramePayload(frame, payload, out var headersPayload);

        // Assert
        Assert.IsTrue(result);
        Assert.ThrowsExactly<HPACKDecodingException>(() => decoder.Decode(headersPayload.FieldBlockFragment));
    }
}
