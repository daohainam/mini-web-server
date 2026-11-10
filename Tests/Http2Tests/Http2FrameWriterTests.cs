using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace Http2Tests;

public class Http2FrameWriterTests
{
    [Fact]
    public void SerializeHEADERSFrame_CreatesValidFrame()
    {
        // Arrange
        uint streamId = 2;
        byte[] fieldBlock = [0x88, 0x0f, 0x10, 0x09, 0x74, 0x65, 0x78, 0x74, 0x2f, 0x70, 0x6c, 0x61, 0x69, 0x6e]; // :status 200, content-type: text/plain
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeHEADERSFrame(streamId, fieldBlock, endHeaders: true, endStream: false, writePayload);

        // Assert
        Assert.Equal(9 + fieldBlock.Length, length);
        Assert.Equal(Http2FrameType.HEADERS, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.END_HEADERS, writePayload[4]);

        uint actualStreamId = (uint)((writePayload[5] & 0x7F) << 24 | writePayload[6] << 16 | writePayload[7] << 8 | writePayload[8]);
        Assert.Equal(streamId, actualStreamId);

        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(fieldBlock.Length, payloadLength);
        Assert.Equal(fieldBlock, writePayload.AsSpan(9, fieldBlock.Length).ToArray());
    }

    [Fact]
    public void SerializeHEADERSFrame_EndStream_SetsBothFlags()
    {
        byte[] writePayload = new byte[1024];

        Http2FrameWriter.SerializeHEADERSFrame(4, [0x88], endHeaders: true, endStream: true, writePayload);

        Assert.Equal((byte)(Http2FrameFlags.END_HEADERS | Http2FrameFlags.END_STREAM), writePayload[4]);
    }

    [Fact]
    public void SerializeHEADERSFrame_WithoutEndHeaders_NoEndHeadersFlag()
    {
        byte[] writePayload = new byte[1024];

        Http2FrameWriter.SerializeHEADERSFrame(6, [0x88], endHeaders: false, endStream: false, writePayload);

        Assert.Equal((byte)Http2FrameFlags.NONE, writePayload[4]);
    }

    [Fact]
    public void WriteHeaderBlock_LargerThanMaxFrameSize_SplitsIntoHeadersAndContinuationFrames()
    {
        // Arrange: a 25 byte block with a max frame size of 10 => HEADERS(10) + CONTINUATION(10) + CONTINUATION(5)
        var block = Enumerable.Range(0, 25).Select(i => (byte)i).ToArray();
        var output = new ArrayBufferWriter<byte>();

        // Act
        Http2FrameWriter.WriteHeaderBlock(output, 7, block, endStream: true, maxFrameSize: 10);

        // Assert
        var buffer = new ReadOnlySequence<byte>(output.WrittenMemory);
        var frame = new Http2Frame();
        var frames = new List<(Http2FrameType Type, Http2FrameFlags Flags, byte[] Payload)>();
        while (Http2FrameReader.TryReadFrame(ref buffer, ref frame, 16384, out var payload))
        {
            Assert.Equal(7u, frame.StreamIdentifier);
            frames.Add((frame.FrameType, frame.Flags, payload.ToArray()));
        }

        Assert.Equal(3, frames.Count);
        Assert.Equal(Http2FrameType.HEADERS, frames[0].Type);
        Assert.Equal(Http2FrameFlags.END_STREAM, frames[0].Flags); // END_HEADERS only on the last frame
        Assert.Equal(Http2FrameType.CONTINUATION, frames[1].Type);
        Assert.Equal(Http2FrameFlags.NONE, frames[1].Flags);
        Assert.Equal(Http2FrameType.CONTINUATION, frames[2].Type);
        Assert.Equal(Http2FrameFlags.END_HEADERS, frames[2].Flags);
        Assert.Equal(block, frames.SelectMany(f => f.Payload).ToArray());
    }

    [Fact]
    public void WriteHeaderBlock_EmptyBlock_WritesOneHeadersFrame()
    {
        var output = new ArrayBufferWriter<byte>();

        Http2FrameWriter.WriteHeaderBlock(output, 1, [], endStream: false, maxFrameSize: 16384);

        Assert.Equal(9, output.WrittenCount);
        Assert.Equal((byte)Http2FrameFlags.END_HEADERS, output.WrittenSpan[4]);
    }

    [Fact]
    public void SerializeRST_STREAMFrame_CreatesValidFrame()
    {
        byte[] writePayload = new byte[32];

        int length = Http2FrameWriter.SerializeRST_STREAMFrame(5, Http2ErrorCode.CANCEL, writePayload);

        Assert.Equal(13, length);
        Assert.Equal("00000403000000000500000008".ToByteArray(), writePayload.AsSpan(0, length).ToArray());
    }

    [Fact]
    public void SerializeGOAWAYFrame_CreatesValidFrame()
    {
        byte[] writePayload = new byte[32];

        int length = Http2FrameWriter.SerializeGOAWAYFrame(30, Http2ErrorCode.COMPRESSION_ERROR, writePayload);

        Assert.Equal(17, length);
        Assert.Equal("0000080700000000000000001E00000009".ToByteArray(), writePayload.AsSpan(0, length).ToArray());
    }

    [Fact]
    public void SerializeWINDOW_UPDATEFrame_CreatesValidFrame()
    {
        byte[] writePayload = new byte[32];

        int length = Http2FrameWriter.SerializeWINDOW_UPDATEFrame(50, 1000, writePayload);

        Assert.Equal(13, length);
        Assert.Equal("000004080000000032000003E8".ToByteArray(), writePayload.AsSpan(0, length).ToArray());
    }

    [Fact]
    public void SerializeWINDOW_UPDATEFrame_ZeroIncrement_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Http2FrameWriter.SerializeWINDOW_UPDATEFrame(1, 0, new byte[32]));
    }

    [Fact]
    public void SerializeDATAFrame_WithData_CreatesValidFrame()
    {
        // Arrange
        uint streamId = 2;
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeDATAFrame(streamId, data, 0, data.Length, true, writePayload);

        // Assert
        Assert.Equal(9 + data.Length, length);
        Assert.Equal(Http2FrameType.DATA, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.END_STREAM, writePayload[4]);
        
        // Verify stream identifier
        uint actualStreamId = (uint)((writePayload[5] & 0x7F) << 24 | writePayload[6] << 16 | writePayload[7] << 8 | writePayload[8]);
        Assert.Equal(streamId, actualStreamId);
        
        // Verify payload length
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(data.Length, payloadLength);
        
        // Verify data is copied correctly
        for (int i = 0; i < data.Length; i++)
        {
            Assert.Equal(data[i], writePayload[9 + i]);
        }
    }

    [Fact]
    public void SerializeDATAFrame_WithEmptyData_CreatesValidFrame()
    {
        // Arrange
        uint streamId = 4;
        byte[] data = [];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeDATAFrame(streamId, data, 0, 0, true, writePayload);

        // Assert
        Assert.Equal(9, length); // Only frame header, no payload
        Assert.Equal(Http2FrameType.DATA, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.END_STREAM, writePayload[4]);
        
        // Verify payload length is 0
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(0, payloadLength);
    }

    [Fact]
    public void SerializeDATAFrame_WithoutEndStream_NoEndStreamFlag()
    {
        // Arrange
        uint streamId = 6;
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Chunk 1");
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeDATAFrame(streamId, data, 0, data.Length, false, writePayload);

        // Assert
        Assert.Equal((byte)Http2FrameFlags.NONE, writePayload[4]);
    }

    [Fact]
    public void SerializeDATAFrame_WithOffset_CopiesCorrectData()
    {
        // Arrange
        uint streamId = 8;
        byte[] data = System.Text.Encoding.UTF8.GetBytes("0123456789");
        int offset = 3;
        int length = 5;
        byte[] writePayload = new byte[1024];

        // Act
        int frameLength = Http2FrameWriter.SerializeDATAFrame(streamId, data, offset, length, true, writePayload);

        // Assert
        Assert.Equal(9 + length, frameLength);
        
        // Verify correct data segment is copied
        byte[] expected = System.Text.Encoding.UTF8.GetBytes("34567");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], writePayload[9 + i]);
        }
    }

    [Fact]
    public void SerializeSETTINGSFrame_WithMultipleSettings_CreatesValidFrame()
    {
        // Arrange
        var frame = new Http2Frame
        {
            FrameType = Http2FrameType.SETTINGS,
            Flags = Http2FrameFlags.NONE,
            StreamIdentifier = 0
        };
        var settings = new[]
        {
            new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE, Value = 16384 },
            new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE, Value = 65535 }
        };
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeSETTINGSFrame(frame, settings, writePayload);

        // Assert
        Assert.Equal(9 + (settings.Length * 6), length); // 9 byte header + 6 bytes per setting
        Assert.Equal(Http2FrameType.SETTINGS, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.NONE, writePayload[4]);
        
        // Verify payload length
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(settings.Length * 6, payloadLength);
        
        // Verify stream identifier is 0 (SETTINGS frames always use stream 0)
        uint streamId = (uint)((writePayload[5] & 0x7F) << 24 | writePayload[6] << 16 | writePayload[7] << 8 | writePayload[8]);
        Assert.Equal(0u, streamId);
    }

    [Fact]
    public void SerializePINGFrame_WithOpaqueData_CreatesValidFrame()
    {
        // Arrange
        var frame = new Http2Frame
        {
            FrameType = Http2FrameType.PING,
            Flags = Http2FrameFlags.NONE,
            StreamIdentifier = 0
        };
        byte[] opaqueData = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializePINGFrame(frame, opaqueData, writePayload);

        // Assert
        Assert.Equal(17, length); // 9 byte header + 8 bytes opaque data
        Assert.Equal(Http2FrameType.PING, (Http2FrameType)writePayload[3]);
        
        // Verify payload length is 8
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(8, payloadLength);
        
        // Verify opaque data is copied correctly
        for (int i = 0; i < opaqueData.Length; i++)
        {
            Assert.Equal(opaqueData[i], writePayload[9 + i]);
        }
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(4, 4)]
    [InlineData(100, 100)]
    [InlineData(1000, 1000)]
    public void SerializeDATAFrame_WithVariousDataSizes_CreatesValidFrames(int dataSize, int expectedDataSize)
    {
        // Arrange
        uint streamId = 2;
        byte[] data = new byte[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (byte)(i % 256);
        }
        byte[] writePayload = new byte[dataSize + 100];

        // Act
        int length = Http2FrameWriter.SerializeDATAFrame(streamId, data, 0, data.Length, true, writePayload);

        // Assert
        Assert.Equal(9 + expectedDataSize, length);
        
        // Verify payload length
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(expectedDataSize, payloadLength);
    }

    [Fact]
    public void SerializeHEADERSFrame_PayloadTooSmall_ThrowsException()
    {
        byte[] writePayload = new byte[5]; // Too small

        Assert.Throws<InternalHttp2Exception>(() =>
            Http2FrameWriter.SerializeHEADERSFrame(2, [0x88], true, true, writePayload));
    }

    [Fact]
    public void SerializeDATAFrame_NullData_ThrowsException()
    {
        // Arrange
        uint streamId = 2;
        byte[] writePayload = new byte[1024];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Http2FrameWriter.SerializeDATAFrame(streamId, null!, 0, 0, true, writePayload));
    }
}
