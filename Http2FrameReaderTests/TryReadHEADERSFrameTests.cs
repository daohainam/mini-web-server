using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;

namespace Http2FrameReaderTests
{
    public class TryReadHEADERSFrameTests
    {
        public class Http2FrameReaderTests
        {
            // some of the test data are from https://github.com/http2jp/http2-frame-test-case


            private static byte[] String2ByteArray(string s)
            {
                byte[] byteArray = new byte[s.Length / 2];
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
                }
                return byteArray;
            }


            [Fact]
            public void TryReadFrame_SETTINGS_ReturnsTrueAndPayload()
            {
                // Arrange
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("00000C040000000000000100002000000300001388"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("00000D010400000001746869732069732064756D6D79"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("000023012C00000003108000001409746869732069732064756D6D79546869732069732070616464696E672E"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("0000140008000000020648656C6C6F2C20776F726C6421486F77647921"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("0000170700000000000000001E00000009687061636B2069732062726F6B656E"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("00000D090000000032746869732069732064756D6D79"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("000000090000000032"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("00000403000000000500000008"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("000004080000000032000003E8"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("000018050C0000000A060000000C746869732069732064756D6D79486F77647921"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("0000050200000000090000000B07"));
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
                var buffer = new ReadOnlySequence<byte>(String2ByteArray("0000080600000000006465616462656566"));
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

            [Fact]
            public void TryReadHEADERSFramePayload_ValidPayload_ReturnsTrueAndHeadersPayload()
            {
                // Arrange
                var frame = new Http2Frame();
                var payload = new ReadOnlySequence<byte>([0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff]);
                var headerTable = new HPACKHeaderTable();
                var logger = NullLogger.Instance;
                Http2FrameHEADERSPayload headersPayload;

                // Act
                var result = Http2FrameReader.TryReadHEADERSFramePayload(ref frame, payload, headerTable, out headersPayload, logger);

                // Assert
                Assert.True(result);
                Assert.NotNull(headersPayload);
                Assert.Equal(4, headersPayload.Headers.Count);
                Assert.Equal(":method", headersPayload.Headers[0].Name);
                Assert.Equal("GET", headersPayload.Headers[0].Value);
                Assert.Equal(":scheme", headersPayload.Headers[1].Name);
                Assert.Equal("http", headersPayload.Headers[1].Value);
                Assert.Equal(":path", headersPayload.Headers[2].Name);
                Assert.Equal("/", headersPayload.Headers[2].Value);
                Assert.Equal(":authority", headersPayload.Headers[3].Name);
                Assert.Equal("www.example.com", headersPayload.Headers[3].Value);
            }
        }
    }
}
