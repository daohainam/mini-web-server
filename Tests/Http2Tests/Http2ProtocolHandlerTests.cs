using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Http2Tests;

/// <summary>
/// Drives the protocol handler the way MiniWebClientConnection does: client frames go into a pipe, server frames land in a MemoryStream.
/// </summary>
public class Http2ProtocolHandlerTests
{
    private sealed class Harness
    {
        private readonly Pipe clientPipe = new();
        private readonly HPACKEncoder clientEncoder = new();

        public Harness(Http2ProtocolHandlerOptions? options = null, long maxRequestBodySize = 10 * 1024 * 1024)
        {
            var loggerFactory = NullLoggerFactory.Instance;
            Handler = new Http2ProtocolHandler(
                new ProtocolHandlerConfiguration(HttpVersions.Http20, maxRequestBodySize),
                loggerFactory,
                new ByteSequenceHttpParser(loggerFactory),
                new ProtocolHandlerContext { PipeReader = clientPipe.Reader, Stream = Output },
                options);
        }

        public Http2ProtocolHandler Handler { get; }
        public MemoryStream Output { get; } = new();

        public async Task SendAsync(byte[] frame)
        {
            await clientPipe.Writer.WriteAsync(frame);
        }

        public Task SendRawAsync(params byte[][] frames) => SendAsync(frames.SelectMany(f => f).ToArray());

        public async Task CloseAsync()
        {
            await clientPipe.Writer.CompleteAsync();
        }

        public byte[] Settings(params Http2FrameSETTINGSItem[] items)
        {
            var buffer = new byte[9 + items.Length * 6];
            Http2FrameWriter.SerializeSETTINGSFrame(new Http2Frame { FrameType = Http2FrameType.SETTINGS }, items, buffer);
            return buffer;
        }

        public byte[] SettingsAck()
        {
            var buffer = new byte[9];
            Http2FrameWriter.SerializeSETTINGSFrame(new Http2Frame { FrameType = Http2FrameType.SETTINGS, Flags = Http2FrameFlags.ACK }, [], buffer);
            return buffer;
        }

        public byte[] Headers(uint streamId, bool endStream, params (string Name, string Value)[] fields)
        {
            var block = new ArrayBufferWriter<byte>();
            clientEncoder.Encode(block, fields.Select(f => new HPACKHeader(f.Name, f.Value)));
            var buffer = new byte[9 + block.WrittenCount];
            Http2FrameWriter.SerializeHEADERSFrame(streamId, block.WrittenSpan, endHeaders: true, endStream, buffer);
            return buffer;
        }

        public byte[] Request(uint streamId, string method = "GET", string path = "/", bool endStream = true, params (string Name, string Value)[] extra)
        {
            (string, string)[] fields = [(":method", method), (":scheme", "http"), (":authority", "localhost:8080"), (":path", path), .. extra];
            return Headers(streamId, endStream, fields);
        }

        public byte[] Data(uint streamId, byte[] data, bool endStream)
        {
            var buffer = new byte[9 + data.Length];
            Http2FrameWriter.SerializeDATAFrame(streamId, data, endStream, buffer);
            return buffer;
        }

        public static byte[] WindowUpdate(uint streamId, uint increment)
        {
            var buffer = new byte[13];
            Http2FrameWriter.SerializeWINDOW_UPDATEFrame(streamId, increment, buffer);
            return buffer;
        }

        public static byte[] RstStream(uint streamId, Http2ErrorCode code)
        {
            var buffer = new byte[13];
            Http2FrameWriter.SerializeRST_STREAMFrame(streamId, code, buffer);
            return buffer;
        }

        public static byte[] Ping(byte[] opaque, bool ack = false)
        {
            var buffer = new byte[17];
            Http2FrameWriter.SerializePINGFrame(new Http2Frame { FrameType = Http2FrameType.PING, Flags = ack ? Http2FrameFlags.ACK : Http2FrameFlags.NONE }, opaque, buffer);
            return buffer;
        }

        public static byte[] Raw(int length, byte type, byte flags, uint streamId, params byte[] payload)
        {
            var buffer = new byte[9 + payload.Length];
            Http2FrameWriter.WriteFrameHeader(buffer, length, (Http2FrameType)type, (Http2FrameFlags)flags, streamId);
            payload.CopyTo(buffer, 9);
            return buffer;
        }

        public async Task<(bool Result, HttpWebRequestBuilder Builder)> ReadRequestAsync(CancellationToken cancellationToken = default)
        {
            var builder = new HttpWebRequestBuilder();
            builder.SetRequestId(1).SetHttps(false).SetPort(8080).SetRemoteAddress(System.Net.IPAddress.Loopback).SetRemotePort(1234);
            var result = await Handler.ReadRequestAsync(builder, cancellationToken);
            return (result, builder);
        }

        public List<(Http2Frame Frame, byte[] Payload)> ReadOutputFrames()
        {
            var frames = new List<(Http2Frame, byte[])>();
            var buffer = new ReadOnlySequence<byte>(Output.ToArray());
            var frame = new Http2Frame();
            while (Http2FrameReader.TryReadFrame(ref buffer, ref frame, 16_777_215, out var payload))
            {
                frames.Add((frame, payload.ToArray()));
                frame = new Http2Frame();
            }

            Assert.Equal(0, buffer.Length);
            return frames;
        }

        public List<HPACKHeader> DecodeResponseHeaders(IEnumerable<(Http2Frame Frame, byte[] Payload)> frames, uint streamId)
        {
            var decoder = new HPACKDecoder();
            var block = frames
                .Where(f => f.Frame.StreamIdentifier == streamId && f.Frame.FrameType is Http2FrameType.HEADERS or Http2FrameType.CONTINUATION)
                .SelectMany(f => f.Frame.FrameType == Http2FrameType.HEADERS
                    ? (Http2FrameReader.TryReadHEADERSFramePayload(f.Frame, new ReadOnlySequence<byte>(f.Payload), out var hp) ? hp.FieldBlockFragment.ToArray() : [])
                    : f.Payload)
                .ToArray();
            return decoder.Decode(new ReadOnlySequence<byte>(block));
        }
    }

    private static Task<T> WithTimeout<T>(Task<T> task) => task.WaitAsync(TimeSpan.FromSeconds(5));
    private static Task WithTimeout(Task task) => task.WaitAsync(TimeSpan.FromSeconds(5));

    private static (Http2Frame Frame, byte[] Payload) Single(List<(Http2Frame Frame, byte[] Payload)> frames, Http2FrameType type)
    {
        return Assert.Single(frames, f => f.Frame.FrameType == type);
    }

    [Fact]
    public async Task Preface_ServerSendsSettingsAndAcksClientSettings()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_HEADER_TABLE_SIZE, Value = 65536 }), h.Request(1));

        var (result, builder) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        var frames = h.ReadOutputFrames();
        Assert.Equal(Http2FrameType.SETTINGS, frames[0].Frame.FrameType);
        Assert.Equal(Http2FrameFlags.NONE, frames[0].Frame.Flags);
        Assert.Equal(0u, frames[0].Frame.StreamIdentifier);
        Assert.Equal(Http2FrameType.SETTINGS, frames[1].Frame.FrameType);
        Assert.Equal(Http2FrameFlags.ACK, frames[1].Frame.Flags);
        Assert.Empty(frames[1].Payload);

        var request = builder.Build();
        Assert.Equal(HttpVersions.Http20, request.HttpVersion);
        Assert.Equal("GET", request.Method.Method);
        Assert.Equal("/", request.Url);
        Assert.Equal("localhost", request.Host);
        Assert.Equal(8080, request.Port);
    }

    [Fact]
    public async Task FirstFrameNotSettings_SendsGoAwayProtocolError()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Request(1));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.False(result);
        var goAway = Single(h.ReadOutputFrames(), Http2FrameType.GOAWAY);
        var payload = new ReadOnlySequence<byte>(goAway.Payload);
        Assert.True(Http2FrameReader.TryReadGOAWAYFramePayload(ref payload, out _, out var code, out _));
        Assert.Equal(Http2ErrorCode.PROTOCOL_ERROR, code);
    }

    [Fact]
    public async Task Response_IsSentOnTheRequestStream_WithOneHeaderBlockAndEndStream()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), h.Request(3));
        var (result, builder) = await WithTimeout(h.ReadRequestAsync());
        Assert.True(result);
        var request = builder.Build();

        var response = new HttpResponse(HttpResponseCodes.OK, h.Output);
        response.Headers.Add("Content-Type", "text/plain");
        response.Headers.Add("Connection", "keep-alive"); // must be dropped
        response.Cookies.Add(new HttpCookie("session", "abc", path: "/"));
        response.Content = new TestContent("hello http/2");

        Assert.True(await WithTimeout(h.Handler.WriteResponseAsync(request, response, CancellationToken.None)));

        var frames = h.ReadOutputFrames().Where(f => f.Frame.StreamIdentifier == 3).ToList();
        Assert.Equal(Http2FrameType.HEADERS, frames[0].Frame.FrameType);
        Assert.Equal(Http2FrameFlags.END_HEADERS, frames[0].Frame.Flags);
        Assert.Single(frames, f => f.Frame.FrameType == Http2FrameType.HEADERS);

        var headers = h.DecodeResponseHeaders(frames, 3);
        Assert.Equal(":status", headers[0].Name);
        Assert.Equal("200", headers[0].Value);
        Assert.Contains(headers, x => x.Name == "content-type" && x.Value == "text/plain");
        Assert.Contains(headers, x => x.Name == "set-cookie" && x.Value == "session=abc; Path=/");
        Assert.DoesNotContain(headers, x => x.Name == "connection");
        Assert.All(headers, x => Assert.Equal(x.Name.ToLowerInvariant(), x.Name));

        var dataFrames = frames.Where(f => f.Frame.FrameType == Http2FrameType.DATA).ToList();
        Assert.Equal("hello http/2", Encoding.ASCII.GetString(dataFrames.SelectMany(f => f.Payload).ToArray()));
        Assert.True(dataFrames[^1].Frame.Flags.HasFlag(Http2FrameFlags.END_STREAM));
        Assert.DoesNotContain(frames, f => f.Frame.FrameType == Http2FrameType.RST_STREAM);
    }

    [Fact]
    public async Task Response_WithoutBody_EndsStreamOnHeaders()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), h.Request(1));
        var (_, builder) = await WithTimeout(h.ReadRequestAsync());
        var request = builder.Build();

        var response = new HttpResponse(HttpResponseCodes.NotFound, h.Output);
        Assert.True(await WithTimeout(h.Handler.WriteResponseAsync(request, response, CancellationToken.None)));

        var frames = h.ReadOutputFrames().Where(f => f.Frame.StreamIdentifier == 1).ToList();
        var headersFrame = Assert.Single(frames);
        Assert.Equal(Http2FrameType.HEADERS, headersFrame.Frame.FrameType);
        Assert.Equal(Http2FrameFlags.END_HEADERS | Http2FrameFlags.END_STREAM, headersFrame.Frame.Flags);
        Assert.Equal(0x8D, headersFrame.Payload[0]); // :status 404 as static index 13
    }

    [Fact]
    public async Task RequestBody_IsDeliveredThroughTheBodyPipe_AndWindowIsReplenished()
    {
        var h = new Harness();
        var body = Encoding.ASCII.GetBytes(new string('x', 50_000));
        await h.SendRawAsync(h.Settings(), h.Request(1, "POST", "/upload", endStream: false, ("content-length", body.Length.ToString()), ("content-type", "text/plain")));

        var (result, builder) = await WithTimeout(h.ReadRequestAsync());
        Assert.True(result);
        var request = builder.Build();
        Assert.Equal(body.Length, request.ContentLength);
        Assert.Equal("text/plain", request.ContentType);

        // body in four DATA frames (each below SETTINGS_MAX_FRAME_SIZE), the demultiplexer keeps running while the next request is awaited;
        // after 48000 bytes more than half of both 65535 byte windows is used, so WINDOW_UPDATEs must go out before END_STREAM arrives
        await h.SendRawAsync(h.Data(1, body[..16_000], false), h.Data(1, body[16_000..32_000], false), h.Data(1, body[32_000..48_000], false), h.Data(1, body[48_000..], true));
        await h.CloseAsync();
        var readTask = h.ReadRequestAsync();

        var received = new MemoryStream();
        await WithTimeout(request.BodyPipeline.Reader.CopyToAsync(received));
        Assert.Equal(body, received.ToArray());

        var (next, _) = await WithTimeout(readTask);
        Assert.False(next);

        var windowUpdates = h.ReadOutputFrames().Where(f => f.Frame.FrameType == Http2FrameType.WINDOW_UPDATE).ToList();
        Assert.Contains(windowUpdates, f => f.Frame.StreamIdentifier == 0);
        Assert.Contains(windowUpdates, f => f.Frame.StreamIdentifier == 1);
    }

    [Fact]
    public async Task RequestBody_LongerThanContentLength_IsResetWithProtocolError()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), h.Request(1, "POST", "/", endStream: false, ("content-length", "2")), h.Data(1, [1, 2, 3], true));
        await h.CloseAsync();

        // the stream is reset before it can be dispatched, so no request surfaces and the (closed) connection has nothing more to offer
        var (result, _) = await WithTimeout(h.ReadRequestAsync());
        Assert.False(result);

        var rst = Single(h.ReadOutputFrames(), Http2FrameType.RST_STREAM);
        Assert.Equal(1u, rst.Frame.StreamIdentifier);
        var payload = new ReadOnlySequence<byte>(rst.Payload);
        Http2FrameReader.TryReadRST_STREAMFramePayload(ref payload, out var code);
        Assert.Equal(Http2ErrorCode.PROTOCOL_ERROR, code);
    }

    [Fact]
    public async Task HeadersWithPaddingAndPriority_AreDecoded()
    {
        var h = new Harness();
        var block = new ArrayBufferWriter<byte>();
        new HPACKEncoder().Encode(block, [new(":method", "GET"), new(":scheme", "https"), new(":authority", "example.com"), new(":path", "/padded?x=1")]);
        var payload = new List<byte> { 4, 0x80, 0, 0, 0, 200 }; // pad length 4, exclusive dependency on stream 0, weight 200
        payload.AddRange(block.WrittenSpan.ToArray());
        payload.AddRange(new byte[4]);
        var frame = Harness.Raw(payload.Count, 0x1, (byte)(Http2FrameFlags.END_HEADERS | Http2FrameFlags.END_STREAM | Http2FrameFlags.PADDED | Http2FrameFlags.PRIORITY), 1, payload.ToArray());

        await h.SendRawAsync(h.Settings(), frame);
        var (result, builder) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        var request = builder.Build();
        Assert.Equal("/padded", request.Url);
        Assert.Equal("?x=1", request.QueryString);
        Assert.Equal("example.com", request.Host);
    }

    [Fact]
    public async Task HeaderBlock_SplitAcrossContinuationFrames_IsReassembled()
    {
        var h = new Harness();
        var block = new ArrayBufferWriter<byte>();
        new HPACKEncoder().Encode(block, [new(":method", "GET"), new(":scheme", "http"), new(":authority", "example.com"), new(":path", "/"), new("x-long", new string('v', 500))]);
        var bytes = block.WrittenSpan.ToArray();
        var headers = Harness.Raw(100, 0x1, (byte)Http2FrameFlags.END_STREAM, 1, bytes[..100]);
        var cont1 = Harness.Raw(100, 0x9, 0, 1, bytes[100..200]);
        var cont2 = Harness.Raw(bytes.Length - 200, 0x9, (byte)Http2FrameFlags.END_HEADERS, 1, bytes[200..]);

        await h.SendRawAsync(h.Settings(), headers, cont1, cont2);
        var (result, builder) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        var request = builder.Build();
        Assert.True(request.Headers.TryGetValue("x-long", out var header));
        Assert.Equal(500, header!.Value.First().Length);
    }

    [Fact]
    public async Task FrameInTheMiddleOfAHeaderBlock_IsAConnectionError()
    {
        var h = new Harness();
        var headers = Harness.Raw(1, 0x1, 0, 1, 0x82); // no END_HEADERS
        await h.SendRawAsync(h.Settings(), headers, Harness.Ping([1, 2, 3, 4, 5, 6, 7, 8]));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.False(result);
        var goAway = Single(h.ReadOutputFrames(), Http2FrameType.GOAWAY);
        var payload = new ReadOnlySequence<byte>(goAway.Payload);
        Http2FrameReader.TryReadGOAWAYFramePayload(ref payload, out _, out var code, out _);
        Assert.Equal(Http2ErrorCode.PROTOCOL_ERROR, code);
    }

    [Fact]
    public async Task MalformedRequest_IsResetAndConnectionStaysUsable()
    {
        var h = new Harness();
        // stream 1: uppercase header name, stream 3: connection header, stream 5: fine
        await h.SendRawAsync(h.Settings(),
            h.Request(1, extra: ("X-Upper", "1")),
            h.Request(3, extra: ("connection", "close")),
            h.Request(5));

        var (result, builder) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        Assert.Equal("/", builder.Build().Url);
        var rsts = h.ReadOutputFrames().Where(f => f.Frame.FrameType == Http2FrameType.RST_STREAM).Select(f => f.Frame.StreamIdentifier).ToList();
        Assert.Equal(new uint[] { 1, 3 }, rsts);
    }

    [Fact]
    public async Task EvenStreamId_IsAConnectionError()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), h.Request(2));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.False(result);
        Single(h.ReadOutputFrames(), Http2FrameType.GOAWAY);
    }

    [Fact]
    public async Task Ping_IsAnsweredWithAck()
    {
        var h = new Harness();
        byte[] opaque = [1, 2, 3, 4, 5, 6, 7, 8];
        await h.SendRawAsync(h.Settings(), Harness.Ping(opaque), h.Request(1));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        var ping = Single(h.ReadOutputFrames(), Http2FrameType.PING);
        Assert.Equal(Http2FrameFlags.ACK, ping.Frame.Flags);
        Assert.Equal(opaque, ping.Payload);
    }

    [Fact]
    public async Task UnknownFrameType_IsIgnored()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), Harness.Raw(3, 0x42, 0xFF, 1, 1, 2, 3), h.Request(1));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(result);
        Assert.DoesNotContain(h.ReadOutputFrames(), f => f.Frame.FrameType is Http2FrameType.GOAWAY or Http2FrameType.RST_STREAM);
    }

    [Fact]
    public async Task InvalidSettings_AreConnectionErrors()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE, Value = 100 }));

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.False(result);
        var goAway = Single(h.ReadOutputFrames(), Http2FrameType.GOAWAY);
        var payload = new ReadOnlySequence<byte>(goAway.Payload);
        Http2FrameReader.TryReadGOAWAYFramePayload(ref payload, out _, out var code, out _);
        Assert.Equal(Http2ErrorCode.PROTOCOL_ERROR, code);
    }

    [Fact]
    public async Task ResponseBody_RespectsFlowControlWindows()
    {
        var h = new Harness();
        // the peer only allows a 1000 byte window per stream and 2000 byte frames
        await h.SendRawAsync(
            h.Settings(new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE, Value = 1000 }, new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE, Value = 16384 }),
            h.Request(1));
        var (_, builder) = await WithTimeout(h.ReadRequestAsync());
        var request = builder.Build();

        var response = new HttpResponse(HttpResponseCodes.OK, h.Output) { Content = new TestContent(new string('a', 2500)) };
        var writeTask = h.Handler.WriteResponseAsync(request, response, CancellationToken.None);

        // the writer must stall after 1000 bytes
        await Task.Delay(200);
        Assert.False(writeTask.IsCompleted);
        var sent = h.ReadOutputFrames().Where(f => f.Frame.FrameType == Http2FrameType.DATA).Sum(f => f.Payload.Length);
        Assert.Equal(1000, sent);

        // the client opens the stream window, the connection window (65535) is still large enough
        await h.SendRawAsync(Harness.WindowUpdate(1, 1500));
        await h.CloseAsync();
        await WithTimeout(h.ReadRequestAsync());

        Assert.True(await WithTimeout(writeTask));
        var dataFrames = h.ReadOutputFrames().Where(f => f.Frame.FrameType == Http2FrameType.DATA).ToList();
        Assert.Equal(2500, dataFrames.Sum(f => f.Payload.Length));
        Assert.All(dataFrames, f => Assert.True(f.Payload.Length <= 16384));
        Assert.True(dataFrames[^1].Frame.Flags.HasFlag(Http2FrameFlags.END_STREAM));
    }

    [Fact]
    public async Task RstStreamFromPeer_AbortsThePendingResponse()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(new Http2FrameSETTINGSItem { Identifier = Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE, Value = 10 }), h.Request(1));
        var (_, builder) = await WithTimeout(h.ReadRequestAsync());
        var request = builder.Build();

        var response = new HttpResponse(HttpResponseCodes.OK, h.Output) { Content = new TestContent(new string('a', 100)) };
        var writeTask = h.Handler.WriteResponseAsync(request, response, CancellationToken.None);
        await Task.Delay(100);
        Assert.False(writeTask.IsCompleted);

        await h.SendRawAsync(Harness.RstStream(1, Http2ErrorCode.CANCEL));
        await h.CloseAsync();
        await WithTimeout(h.ReadRequestAsync());

        Assert.True(await WithTimeout(writeTask)); // abandoned, not failed
        Assert.Equal(10, h.ReadOutputFrames().Where(f => f.Frame.FrameType == Http2FrameType.DATA).Sum(f => f.Payload.Length));
    }

    [Fact]
    public async Task GoAwayFromPeer_StopsReadingOnceStreamsAreDone()
    {
        var h = new Harness();
        var goAway = Harness.Raw(8, 0x7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        await h.SendRawAsync(h.Settings(), goAway);

        var (result, _) = await WithTimeout(h.ReadRequestAsync());

        Assert.False(result);
        Assert.DoesNotContain(h.ReadOutputFrames(), f => f.Frame.FrameType == Http2FrameType.GOAWAY);
    }

    [Fact]
    public async Task HeadRequest_NeverGetsDataFrames()
    {
        var h = new Harness();
        await h.SendRawAsync(h.Settings(), h.Request(1, "HEAD"));
        var (_, builder) = await WithTimeout(h.ReadRequestAsync());
        var request = builder.Build();
        Assert.Equal("HEAD", request.Method.Method);

        var response = new HttpResponse(HttpResponseCodes.OK, h.Output) { Content = new TestContent("body") };
        await WithTimeout(h.Handler.WriteResponseAsync(request, response, CancellationToken.None));

        var frames = h.ReadOutputFrames().Where(f => f.Frame.StreamIdentifier == 1).ToList();
        var only = Assert.Single(frames);
        Assert.Equal(Http2FrameType.HEADERS, only.Frame.FrameType);
        Assert.True(only.Frame.Flags.HasFlag(Http2FrameFlags.END_STREAM));
    }

    [Fact]
    public async Task SecondRequest_UsesTheDynamicTableFilledByTheFirst()
    {
        var h = new Harness();
        // the harness encoder never indexes, so build a block by hand: first request indexes :authority, second refers to it (RFC 7541 C.3)
        var first = Harness.Raw(20, 0x1, (byte)(Http2FrameFlags.END_HEADERS | Http2FrameFlags.END_STREAM), 1, "828684410f7777772e6578616d706c652e636f6d".ToByteArray());
        var second = Harness.Raw(14, 0x1, (byte)(Http2FrameFlags.END_HEADERS | Http2FrameFlags.END_STREAM), 3, "828684be58086e6f2d6361636865".ToByteArray());
        await h.SendRawAsync(h.Settings(), first, second);

        var (r1, b1) = await WithTimeout(h.ReadRequestAsync());
        var (r2, b2) = await WithTimeout(h.ReadRequestAsync());

        Assert.True(r1 && r2);
        Assert.Equal("www.example.com", b1.Build().Host);
        var request2 = b2.Build();
        Assert.Equal("www.example.com", request2.Host);
        Assert.Equal("no-cache", request2.Headers.CacheControl);
    }

    [Fact]
    public async Task RequestBody_ReadByTheApplicationWhileFramesArrive_IsComplete()
    {
        var h = new Harness();
        const int bodyLength = 200_000;
        var body = Encoding.ASCII.GetBytes(new string('y', bodyLength));
        await h.SendRawAsync(h.Settings(), h.Request(1, "POST", "/", endStream: false, ("content-length", bodyLength.ToString()), ("content-type", "text/plain")));

        var (result, builder) = await WithTimeout(h.ReadRequestAsync());
        Assert.True(result);
        var request = builder.Build();

        // the application reads the body through the request helper while DATA frames keep arriving in small pieces
        var readTask = request.ReadAsStringAsync(CancellationToken.None);
        var demux = Task.Run(async () =>
        {
            var next = h.ReadRequestAsync();
            for (int offset = 0; offset < bodyLength;)
            {
                int chunk = Math.Min(1500, bodyLength - offset);
                await h.SendAsync(h.Data(1, body[offset..(offset + chunk)], offset + chunk >= bodyLength));
                offset += chunk;
                await Task.Delay(1);
            }

            await h.CloseAsync();
            return await next;
        });

        var text = await WithTimeout(readTask);
        Assert.Equal(bodyLength, text.Length);
        await WithTimeout(demux);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task RequestBody_ReadByTheApplication_StressWithRandomChunks(int seed)
    {
        var random = new Random(seed);
        for (int iteration = 0; iteration < 10; iteration++)
        {
            var h = new Harness();
            int bodyLength = 50_000 + random.Next(100_000);
            var body = new byte[bodyLength];
            random.NextBytes(body);
            await h.SendRawAsync(h.Settings(), h.Request(1, "POST", "/", endStream: false, ("content-length", bodyLength.ToString()), ("content-type", "application/octet-stream")));

            var (result, builder) = await WithTimeout(h.ReadRequestAsync());
            Assert.True(result);
            var request = builder.Build();

            var readTask = Task.Run(async () =>
            {
                var received = new MemoryStream();
                var reader = request.BodyPipeline.Reader;
                while (true)
                {
                    var readResult = await reader.ReadAsync();
                    foreach (var segment in readResult.Buffer)
                    {
                        received.Write(segment.Span);
                    }

                    reader.AdvanceTo(readResult.Buffer.End);
                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }

                return received.ToArray();
            });

            var demux = Task.Run(async () =>
            {
                var next = h.ReadRequestAsync();
                for (int offset = 0; offset < bodyLength;)
                {
                    int chunk = Math.Min(1 + random.Next(16_384), bodyLength - offset);
                    await h.SendAsync(h.Data(1, body[offset..(offset + chunk)], offset + chunk >= bodyLength));
                    offset += chunk;
                }

                await h.CloseAsync();
                return await next;
            });

            Assert.Equal(body, await WithTimeout(readTask));
            await WithTimeout(demux);
        }
    }

    private sealed class TestContent(string text) : IHttpContent
    {
        private readonly byte[] bytes = Encoding.ASCII.GetBytes(text);

        public MiniWebServer.Abstractions.Http.HttpHeaders Headers { get; } = new("Content-Length", text.Length.ToString());

        public async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            // write in two chunks so that chunking and flow control get exercised
            int half = bytes.Length / 2;
            await stream.WriteAsync(bytes.AsMemory(0, half), cancellationToken);
            await stream.WriteAsync(bytes.AsMemory(half), cancellationToken);
            return bytes.Length;
        }
    }
}
