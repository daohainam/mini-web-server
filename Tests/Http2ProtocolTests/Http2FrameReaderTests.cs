using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace MiniWebServer.Server.Tests.ProtocolHandlers.Http2
{
    [TestClass()]
    public class Http2FrameReaderTests
    {
        [TestMethod()]
        public void TryReadHEADERSFramePayload_ValidPayload_ReturnsTrueAndHeadersPayload()
        {
            // Arrange
            var frame = new Http2Frame();

            // from https://httpwg.org/specs/rfc7541.html#n-first-request_2
            var payload = new ReadOnlySequence<byte>(new byte[] { 0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff });
            var headerTable = new HPACKHeaderTable();
            var logger = NullLogger.Instance;

            // Act
            var result = Http2FrameReader.TryReadHEADERSFramePayload(ref frame, payload, headerTable, out var headersPayload, logger);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(headersPayload);
            Assert.AreEqual(2, headersPayload.Headers.Count);
            Assert.AreEqual("AB", headersPayload.Headers[0].Name);
            Assert.AreEqual("C", headersPayload.Headers[0].Value);
            Assert.AreEqual("ABC", headersPayload.Headers[1].Name);
            Assert.AreEqual("", headersPayload.Headers[1].Value);
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
}
