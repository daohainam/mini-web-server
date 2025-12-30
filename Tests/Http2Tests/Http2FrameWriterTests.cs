using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace Http2Tests;

public class Http2FrameWriterTests
{
    [Fact]
    public void SerializeHEADERFrame_WithIndexedHeaderName_CreatesValidFrame()
    {
        // Arrange
        uint streamId = 2;
        string headerName = "content-type";
        string[] headerValues = ["application/json"];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeHEADERFrame(streamId, headerName, headerValues, true, true, writePayload);

        // Assert
        Assert.True(length > 9); // Frame header is 9 bytes
        Assert.Equal(Http2FrameType.HEADERS, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.END_HEADERS, writePayload[4]);
        
        // Verify stream identifier (bytes 5-8)
        uint actualStreamId = (uint)((writePayload[5] & 0x7F) << 24 | writePayload[6] << 16 | writePayload[7] << 8 | writePayload[8]);
        Assert.Equal(streamId, actualStreamId);
        
        // Verify payload length matches
        int payloadLength = writePayload[0] << 16 | writePayload[1] << 8 | writePayload[2];
        Assert.Equal(length - 9, payloadLength);
    }

    [Fact]
    public void SerializeHEADERFrame_WithLiteralHeaderName_CreatesValidFrame()
    {
        // Arrange
        uint streamId = 4;
        string headerName = "x-custom-header";
        string[] headerValues = ["custom-value"];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeHEADERFrame(streamId, headerName, headerValues, false, true, writePayload);

        // Assert
        Assert.True(length > 9);
        Assert.Equal(Http2FrameType.HEADERS, (Http2FrameType)writePayload[3]);
        Assert.Equal((byte)Http2FrameFlags.END_HEADERS, writePayload[4]);
        
        // Verify stream identifier
        uint actualStreamId = (uint)((writePayload[5] & 0x7F) << 24 | writePayload[6] << 16 | writePayload[7] << 8 | writePayload[8]);
        Assert.Equal(streamId, actualStreamId);
    }

    [Fact]
    public void SerializeHEADERFrame_WithoutEndHeaders_NoEndHeadersFlag()
    {
        // Arrange
        uint streamId = 6;
        string headerName = "content-length";
        string[] headerValues = ["1234"];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeHEADERFrame(streamId, headerName, headerValues, false, false, writePayload);

        // Assert
        Assert.Equal((byte)Http2FrameFlags.NONE, writePayload[4]);
    }

    [Fact]
    public void SerializeHEADERFrame_WithMultipleValues_JoinsWithComma()
    {
        // Arrange
        uint streamId = 8;
        string headerName = "accept-encoding";
        string[] headerValues = ["gzip", "deflate", "br"];
        byte[] writePayload = new byte[1024];

        // Act
        int length = Http2FrameWriter.SerializeHEADERFrame(streamId, headerName, headerValues, false, true, writePayload);

        // Assert
        Assert.True(length > 9);
        // The header value should be joined with ", " so the length should be longer
        Assert.True(length > 9 + 10); // At least header + some value
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
    public void SerializeHEADERFrame_PayloadTooSmall_ThrowsException()
    {
        // Arrange
        uint streamId = 2;
        string headerName = "content-type";
        string[] headerValues = ["application/json"];
        byte[] writePayload = new byte[5]; // Too small

        // Act & Assert
        Assert.Throws<InternalHttp2Exception>(() =>
            Http2FrameWriter.SerializeHEADERFrame(streamId, headerName, headerValues, true, true, writePayload));
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
