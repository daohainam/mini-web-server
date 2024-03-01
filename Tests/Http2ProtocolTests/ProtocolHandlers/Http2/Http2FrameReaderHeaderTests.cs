using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

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

        [TestMethod()]
        [DynamicData(nameof(TryReadFrameTestBuffers), DynamicDataSourceType.Method)]
        public void TryReadFrameTest(byte[] buffer, int length, int frameType, byte flags, int streamId)
        {
            Http2Frame frame = new();
            var rbuffer = new ReadOnlySequence<byte>(buffer);
            var b = Http2FrameReader.TryReadFrame(ref rbuffer, ref frame, 16384);

            Assert.IsTrue(b);
            Assert.IsTrue(frame != null);
            Assert.AreEqual(length, frame.Length);
            Assert.AreEqual(flags, frame.Flags);
            Assert.AreEqual(frameType, (int)frame.FrameType);
            Assert.AreEqual(streamId, frame.StreamIdentifier);
        }

        private static IEnumerable<object[]> TryReadFrameTestBuffers()
        {
            var buffers = new List<object[]>
            {
                (
                [

                    new byte[] { 0x00, 0x00, 0xFF,
                        0x01, // HEADER_TYPE
                        0x00,
                        0x70, 0xFF, 0x00, 0xFF
                    },
                    0x0000FF, 0x01, (byte)0x00, 0x70FF00FF
                ]
                )
            };

            return buffers;
        }
    }
}