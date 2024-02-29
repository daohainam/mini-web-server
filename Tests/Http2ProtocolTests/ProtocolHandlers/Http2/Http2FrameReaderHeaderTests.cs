namespace MiniWebServer.Server.ProtocolHandlers.Http2.Tests
{
    [TestClass()]
    public class Http2FrameReaderHeaderTests
    {
        [TestMethod()]
        [DataRow((byte)0x20, (byte)0x30, (byte)0x40, 0x203040)]
        [DataRow((byte)0xFF, (byte)0xFF, (byte)0xFF, 0xFFFFFF)]
        [DataRow((byte)0x00, (byte)0x00, (byte)0x00, 0x000000)]
        public void ReadHeaderLengthTest(byte b1, byte b2, byte b3, int expected)
        {
            var buffer = new byte[3] { b1, b2, b3 };
            var length = Http2FrameReader.ReadHeaderLength(buffer);

            Assert.AreEqual(expected, length);
        }
    }
}