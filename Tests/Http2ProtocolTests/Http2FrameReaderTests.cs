using Microsoft.Extensions.Logging.Abstractions;
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
        var frame = new Http2Frame();

        // from https://httpwg.org/specs/rfc7541.html#n-first-request_2
        var payload = new ReadOnlySequence<byte>([0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff]);
        var headerTable = new HPACKHeaderTable();
        var logger = NullLogger.Instance;

        // Act
        var result = Http2FrameReader.TryReadHEADERSFramePayload(ref frame, payload, headerTable, out var headersPayload, logger);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(headersPayload);
        Assert.AreEqual(4, headersPayload.Headers.Count);
        Assert.AreEqual(":method", headersPayload.Headers[0].Name);
        Assert.AreEqual("GET", headersPayload.Headers[0].Value);
        Assert.AreEqual(":scheme", headersPayload.Headers[1].Name);
        Assert.AreEqual("http", headersPayload.Headers[1].Value);
        Assert.AreEqual(":path", headersPayload.Headers[2].Name);
        Assert.AreEqual("/", headersPayload.Headers[2].Value);
        Assert.AreEqual(":authority", headersPayload.Headers[3].Name);
        Assert.AreEqual("www.example.com", headersPayload.Headers[3].Value);
    }

    //[TestMethod()]
    //public void TryReadHEADERSFramePayload_InvalidPayload_ReturnsFalseAndEmptyHeadersPayload()
    //{
    //    // Arrange
    //    var frame = new Http2Frame();
    //    var payload = new ReadOnlySequence<byte>(new byte[] { 0x00 });
    //    var headerTable = new HPACKHeaderTable();
    //    var logger = NullLogger.Instance;

    //    // Act
    //    var result = Http2FrameReader.TryReadHEADERSFramePayload(ref frame, payload, headerTable, out var headersPayload, logger);

    //    // Assert
    //    Assert.IsFalse(result);
    //    Assert.IsNotNull(headersPayload);
    //    //Assert.Empty(headersPayload.Headers);
    //}
}
