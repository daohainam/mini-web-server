using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace Http2Tests;

public class TryReadFrameTests
{
    public class Http2FrameReaderTests
    { 
        // some of the test data are from https://github.com/http2jp/http2-frame-test-case

        [Fact]
        public void TryReadFrame_SETTINGS_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("00000C040000000000000100002000000300001388".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.SETTINGS, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(12, frame.Length);
            Assert.Equal(0u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_HEADERS_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("00000D010400000001746869732069732064756D6D79".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.HEADERS, frame.FrameType);
            Assert.Equal(Http2FrameFlags.END_HEADERS, frame.Flags);
            Assert.Equal(13, frame.Length);
            Assert.Equal(1u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_HEADERS_PRIORITY_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("000023012C00000003108000001409746869732069732064756D6D79546869732069732070616464696E672E".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.HEADERS, frame.FrameType);
            Assert.Equal(Http2FrameFlags.END_HEADERS | Http2FrameFlags.PRIORITY | Http2FrameFlags.PADDED, frame.Flags);
            Assert.Equal(35, frame.Length);
            Assert.Equal(3u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_DATA_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("0000140008000000020648656C6C6F2C20776F726C6421486F77647921".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.DATA, frame.FrameType);
            Assert.Equal(Http2FrameFlags.PADDED, frame.Flags);
            Assert.Equal(20, frame.Length);
            Assert.Equal(2u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_GOAWAY_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("0000170700000000000000001E00000009687061636B2069732062726F6B656E".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.GOAWAY, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(23, frame.Length);
            Assert.Equal(0u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_CONTINUATION_HEADER_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("00000D090000000032746869732069732064756D6D79".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.CONTINUATION, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(13, frame.Length);
            Assert.Equal(50u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_CONTINUATION_NORMAL_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("000000090000000032".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.CONTINUATION, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(0, frame.Length);
            Assert.Equal(50u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_RST_STREAM_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("00000403000000000500000008".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.RST_STREAM, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(4, frame.Length);
            Assert.Equal(5u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_WINDOW_UPDATE_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("000004080000000032000003E8".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.WINDOW_UPDATE, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(4, frame.Length);
            Assert.Equal(50u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_PUSH_PROMISE_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("000018050C0000000A060000000C746869732069732064756D6D79486F77647921".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.PUSH_PROMISE, frame.FrameType);
            Assert.Equal(Http2FrameFlags.END_HEADERS | Http2FrameFlags.PADDED, frame.Flags);
            Assert.Equal(24, frame.Length);
            Assert.Equal(10u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_PRIORITY_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("0000050200000000090000000B07".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.PRIORITY, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(5, frame.Length);
            Assert.Equal(9u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_PING_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>("0000080600000000006465616462656566".ToByteArray());
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.PING, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(8, frame.Length);
            Assert.Equal(0u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        [Fact]
        public void TryReadFrame_HEADER_ReturnsTrueAndPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>([0x0, 0x0, 0x27, 0x1, 0x5, 0x0, 0x0, 0x0, 0x1, 0x82, 0x86, 0x1, 0xe, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x68, 0x6f, 0x73, 0x74, 0x3a, 0x39, 0x36, 0x35, 0x32, 0x84, 0xf, 0x1, 0x11, 0x67, 0x7a, 0x69, 0x70, 0x2c, 0x20, 0x64, 0x65, 0x66, 0x6c, 0x61, 0x74, 0x65, 0x2c, 0x20, 0x62, 0x72]);
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload);

            // Assert
            Assert.True(result);
            Assert.Equal(Http2FrameType.HEADERS, frame.FrameType);
            Assert.Equal(Http2FrameFlags.END_STREAM | Http2FrameFlags.END_HEADERS, frame.Flags);
            Assert.Equal(39, frame.Length);
            Assert.Equal(1u, frame.StreamIdentifier);
            Assert.Equal(frame.Length, payload.Length);
        }

        //------------------------------------------------------------------


        [Fact]
        public void TryReadFrame_InvalidBuffer_ReturnsFalseAndEmptyPayload()
        {
            // Arrange
            var buffer = new ReadOnlySequence<byte>([0x00]);
            var frame = new Http2Frame();
            var maxFrameSize = 1024u;
            ReadOnlySequence<byte> payload;

            // Act
            var result = Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out payload);

            // Assert
            Assert.False(result);
            Assert.Equal(0, frame.Length);
            Assert.Equal(Http2FrameType.DATA, frame.FrameType);
            Assert.Equal(Http2FrameFlags.NONE, frame.Flags);
            Assert.Equal(0u, frame.StreamIdentifier);
            Assert.Equal(0, payload.Length);
        }

        [Fact]
        public void ReadPayloadLength_ValidHeader_ReturnsPayloadLength()
        {
            // Arrange
            var header = new byte[] { 0x99, 0xFF, 0xEE };
            var expectedPayloadLength = 0x99FFEE;

            // Act
            var payloadLength = Http2FrameReader.ReadPayloadLength(header);

            // Assert
            Assert.Equal(expectedPayloadLength, payloadLength);
        }

        [Fact]
        public void GetStreamIdentifier_ValidHeader_ReturnsStreamIdentifier()
        {
            // Arrange
            var header = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x34, 0x56 };
            var expectedStreamIdentifier = 0x123456u;

            // Act
            var streamIdentifier = Http2FrameReader.GetStreamIdentifier(header);

            // Assert
            Assert.Equal(expectedStreamIdentifier, streamIdentifier);
        }

    }
}
