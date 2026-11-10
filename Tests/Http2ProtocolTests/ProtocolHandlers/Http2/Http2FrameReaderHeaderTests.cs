using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2.Tests;

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
        var length = Http2FrameReader.ReadPayloadLength(buffer);

        Assert.AreEqual(expected, length);
    }

    [TestMethod()]
    [DynamicData(nameof(TryReadFrameTestBuffers), DynamicDataSourceType.Method)]
    public void TryReadFrameTest(byte[] buffer, int length, int frameType, byte flags, uint streamId)
    {
        Http2Frame frame = new();
        var rbuffer = new ReadOnlySequence<byte>(buffer);
        var b = Http2FrameReader.TryReadFrame(ref rbuffer, ref frame, 16384, out var payload);

        Assert.IsTrue(b);
        Assert.IsTrue(frame != null);
        Assert.AreEqual(length, frame.Length);
        Assert.AreEqual((Http2FrameFlags)flags, frame.Flags);
        Assert.AreEqual(frameType, (int)frame.FrameType);
        Assert.AreEqual(streamId, frame.StreamIdentifier);
    }

    private static IEnumerable<object[]> TryReadFrameTestBuffers()
    {
        var buffers = new List<object[]>
        {
            (
            [
                new byte[] { 0x00, 0x00, 0x00,
                    0x01, // HEADER_TYPE
                    0x00,
                    0x70, 0xFF, 0x00, 0xFF
                },
                0x000000, 0x01, (byte)0x00, 0x70FF00FFu
            ]
            )
        };

        return buffers;
    }

    [TestMethod()]
    public void TryReadFrame_UnknownFrameType_IsReturnedNotRejected()
    {
        // RFC 9113 4.1: unknown frame types must be ignored and discarded, so the reader must not throw on them
        Http2Frame frame = new();
        var buffer = new ReadOnlySequence<byte>([0x00, 0x00, 0x01, 0xAB, 0x00, 0x00, 0x00, 0x00, 0x01, 0xFF]);

        var b = Http2FrameReader.TryReadFrame(ref buffer, ref frame, 16384, out var payload);

        Assert.IsTrue(b);
        Assert.AreEqual(0xAB, (byte)frame.FrameType);
        Assert.AreEqual(1, payload.Length);
        Assert.AreEqual(0, buffer.Length);
    }

    [TestMethod()]
    public void TryReadFrame_PayloadLargerThanMaxFrameSize_ThrowsFrameSizeError()
    {
        Http2Frame frame = new();
        var buffer = new ReadOnlySequence<byte>([0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]); // length 16385

        var ex = Assert.ThrowsExactly<Http2ConnectionException>(() => Http2FrameReader.TryReadFrame(ref buffer, ref frame, 16384, out _));
        Assert.AreEqual(Http2ErrorCode.FRAME_SIZE_ERROR, ex.ErrorCode);
    }

    [TestMethod()]
    [DataRow(new byte[] { 0b_0000_1010 }, 5, 10)]
    [DataRow(new byte[] { 0b_0001_1111, 0b_1001_1010, 0b_0000_1010 }, 5, 1337)]
    [DataRow(new byte[] { 0b_0010_1010 }, 8, 42)]
    public void DecodeIntegerTest(byte[] bytes, int prefixBits, int expected)
    {
        int offset = 0;

        Assert.AreEqual(expected, HPACKInteger.Read(bytes, ref offset, prefixBits));
        Assert.AreEqual(bytes.Length, offset);
    }

    [TestMethod()]
    public void TryReadHEADERSFramePayloadTest()
    {
        // PADDED: pad length 2, block = :method GET (0x82), :path / (0x84), two bytes of padding
        var frame = new Http2Frame { FrameType = Http2FrameType.HEADERS, Flags = Http2FrameFlags.END_HEADERS | Http2FrameFlags.PADDED };
        var payload = new ReadOnlySequence<byte>([0x02, 0x82, 0x84, 0x00, 0x00]);

        var result = Http2FrameReader.TryReadHEADERSFramePayload(frame, payload, out var headersPayload);

        Assert.IsTrue(result);
        Assert.AreEqual(2, headersPayload.PadLength);
        Assert.IsFalse(headersPayload.HasPriority);
        CollectionAssert.AreEqual(new byte[] { 0x82, 0x84 }, headersPayload.FieldBlockFragment.ToArray());
    }
}
