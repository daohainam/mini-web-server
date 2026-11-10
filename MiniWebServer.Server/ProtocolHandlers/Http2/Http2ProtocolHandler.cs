using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/*
 * HTTP/2 protocol handler based on RFC 9113 (https://www.rfc-editor.org/rfc/rfc9113) and HPACK, RFC 7541.
 *
 * How it fits into the connection loop:
 * - ReadRequestAsync runs the frame demultiplexer. It returns true as soon as one stream has a complete request header block, the
 *   caller builds and dispatches the request and calls ReadRequestAsync again. DATA frames arriving later for that stream are written
 *   to the stream's body pipe by the demultiplexer, so ReadBodyAsync has nothing to do.
 * - WriteResponseAsync is called concurrently from the dispatched requests, the response goes to the stream the request came from.
 *   All frame writes are serialized through a single lock so frames never interleave.
 * - Connection errors send GOAWAY and make ReadRequestAsync return false, stream errors send RST_STREAM and keep the connection alive.
 *
 * Not supported: server push (SETTINGS_ENABLE_PUSH is always 0), the deprecated priority scheme (PRIORITY frames are validated and ignored),
 * the CONNECT method and the h2c Upgrade mechanism (deprecated by RFC 9113, only the connection preface / "prior knowledge" is used).
 */

public partial class Http2ProtocolHandler : IProtocolHandler
{
    private readonly ILogger<Http2ProtocolHandler> logger;
    private readonly ProtocolHandlerConfiguration config;
    private readonly IHttpComponentParser httpComponentParser;
    private readonly ProtocolHandlerContext protocolHandlerContext;
    private readonly Http2ProtocolHandlerOptions options;

    // peer settings, these govern what we send (RFC 9113 6.5.2)
    private uint peerMaxFrameSize = Http2ProtocolHandlerOptions.DefaultMaxFrameSize;
    private uint peerInitialWindowSize = Http2ProtocolHandlerOptions.DefaultInitialWindowSize;
    private uint peerMaxConcurrentStreams = uint.MaxValue; // only matters for server push, which we do not do
    private uint? peerMaxHeaderListSize;

    // flow control (RFC 9113 6.9), every window is guarded by flowControlLock
    private readonly object flowControlLock = new();
    private long connectionSendWindow = Http2ProtocolHandlerOptions.DefaultInitialWindowSize;
    private long connectionReceiveWindow;
    private long unacknowledgedConnectionBytes;
    private TaskCompletionSource windowUpdated = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // streams (RFC 9113 5.1)
    private readonly ConcurrentDictionary<uint, Http2Stream> streams = new();
    private readonly ConcurrentDictionary<Pipe, Http2Stream> streamsByBody = new();
    private readonly Queue<Http2Stream> readyStreams = new(); // complete requests waiting to be handed to the connection, read loop only
    private uint lastClientStreamId; // the highest client stream id seen so far, used to tell idle from closed streams

    // a header block in progress: HEADERS followed by CONTINUATION frames (RFC 9113 4.3)
    private Http2Stream? headerBlockStream;
    private bool headerBlockEndStream;
    private readonly ArrayBufferWriter<byte> headerBlock = new();

    // HPACK contexts, one per direction
    private readonly HPACKDecoder decoder;
    private readonly HPACKEncoder encoder = new();

    // connection state
    private readonly SemaphoreSlim writeLock = new(1, 1);
    private bool prefaceSent;
    private bool firstFrameReceived;
    private bool goAwayReceived;
    private bool goAwaySent;
    private bool connectionClosed;

    public Http2ProtocolHandler(ProtocolHandlerConfiguration config, ILoggerFactory loggerFactory, IHttpComponentParser httpComponentParser, ProtocolHandlerContext protocolHandlerContext, Http2ProtocolHandlerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.httpComponentParser = httpComponentParser ?? throw new ArgumentNullException(nameof(httpComponentParser));
        this.protocolHandlerContext = protocolHandlerContext ?? throw new ArgumentNullException(nameof(protocolHandlerContext));
        this.options = options ?? new Http2ProtocolHandlerOptions();

        logger = loggerFactory.CreateLogger<Http2ProtocolHandler>();

        connectionReceiveWindow = Http2ProtocolHandlerOptions.DefaultInitialWindowSize; // the connection window always starts at the default, SETTINGS do not change it
        decoder = new HPACKDecoder(new HPACKHeaderTable((int)this.options.HeaderTableSize), (int)this.options.MaxHeaderListSize, this.options.MaxHeaderCount);
    }

    public int ProtocolVersion => 20;

    public Task ReadBodyAsync(PipeReader reader, IHttpRequest request, CancellationToken cancellationToken)
    {
        // request bodies are delivered by the frame demultiplexer in ReadRequestAsync, there is nothing to pull here
        return Task.CompletedTask;
    }

    public void Reset()
    {
    }

    /// <summary>
    /// Runs the frame demultiplexer until one stream has a complete request (true), or until the connection must be closed (false).
    /// A false result means GOAWAY was already sent when the failure was a protocol error.
    /// </summary>
    public async Task<bool> ReadRequestAsync(IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestBuilder);

        try
        {
            if (!prefaceSent)
            {
                // RFC 9113 3.4: the server connection preface consists of a potentially empty SETTINGS frame
                await SendServerPrefaceAsync(cancellationToken);
                prefaceSent = true;
            }

            var frame = new Http2Frame();

            while (true)
            {
                while (readyStreams.TryDequeue(out var readyStream))
                {
                    if (await TryBuildRequestAsync(readyStream, requestBuilder, cancellationToken))
                    {
                        return true;
                    }
                }

                if (connectionClosed || (goAwayReceived && streams.IsEmpty))
                {
                    return false;
                }

                ReadResult readResult = await protocolHandlerContext.PipeReader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                try
                {
                    while (Http2FrameReader.TryReadFrame(ref buffer, ref frame, options.MaxFrameSize, out ReadOnlySequence<byte> payload))
                    {
                        try
                        {
                            await ProcessFrameAsync(frame, payload, cancellationToken);
                        }
                        catch (Http2StreamException ex)
                        {
                            await HandleStreamErrorAsync(ex, cancellationToken);
                        }
                    }
                }
                catch (Http2ConnectionException ex)
                {
                    protocolHandlerContext.PipeReader.AdvanceTo(buffer.Start);
                    logger.LogError("HTTP/2 connection error {code}: {message}", ex.ErrorCode, ex.Message);
                    await TrySendGoAwayAsync(ex.ErrorCode, cancellationToken);

                    return false;
                }

                // everything up to buffer.Start is consumed, the remainder (a partial frame) has been examined
                protocolHandlerContext.PipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && readyStreams.Count == 0)
                {
                    // the peer will not send anything more, responses already in flight are still written by their own tasks
                    logger.LogDebug("Peer closed the connection");

                    return false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Connection expired");
            await TrySendGoAwayAsync(Http2ErrorCode.NO_ERROR, CancellationToken.None);

            return false;
        }
        catch (IOException ex)
        {
            logger.LogDebug(ex, "I/O error");
            Shutdown();

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading request");
            await TrySendGoAwayAsync(Http2ErrorCode.INTERNAL_ERROR, CancellationToken.None);

            return false;
        }
    }

    private async Task ProcessFrameAsync(Http2Frame frame, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Frame received: {ft}, stream: {sid}, length: {len}, flags: {flags}", frame.FrameType, frame.StreamIdentifier, frame.Length, frame.Flags);
        }

        if (!firstFrameReceived)
        {
            // RFC 9113 3.4: the client preface must be followed by a SETTINGS frame
            if (frame.FrameType != Http2FrameType.SETTINGS || frame.Flags.HasFlag(Http2FrameFlags.ACK))
            {
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "The first frame must be a SETTINGS frame");
            }

            firstFrameReceived = true;
        }

        if (headerBlockStream != null && (frame.FrameType != Http2FrameType.CONTINUATION || frame.StreamIdentifier != headerBlockStream.StreamId))
        {
            // RFC 9113 4.3: a header block must be a contiguous sequence of frames on the same stream
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"Expected a CONTINUATION frame for stream {headerBlockStream.StreamId}, got {frame.FrameType} for stream {frame.StreamIdentifier}");
        }

        switch (frame.FrameType)
        {
            case Http2FrameType.DATA:
                await ProcessDATAFrameAsync(frame, payload, cancellationToken);
                break;
            case Http2FrameType.HEADERS:
                ProcessHEADERSFrame(frame, payload);
                break;
            case Http2FrameType.PRIORITY:
                ProcessPRIORITYFrame(frame, payload);
                break;
            case Http2FrameType.RST_STREAM:
                ProcessRST_STREAMFrame(frame, payload);
                break;
            case Http2FrameType.SETTINGS:
                await ProcessSETTINGSFrameAsync(frame, payload, cancellationToken);
                break;
            case Http2FrameType.PUSH_PROMISE:
                // RFC 9113 8.4: clients cannot push
                throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "Received a PUSH_PROMISE frame from the client");
            case Http2FrameType.PING:
                await ProcessPINGFrameAsync(frame, payload, cancellationToken);
                break;
            case Http2FrameType.GOAWAY:
                ProcessGOAWAYFrame(frame, payload);
                break;
            case Http2FrameType.WINDOW_UPDATE:
                ProcessWINDOW_UPDATEFrame(frame, payload);
                break;
            case Http2FrameType.CONTINUATION:
                ProcessCONTINUATIONFrame(frame, payload);
                break;
            default:
                // RFC 9113 4.1: implementations must ignore and discard frames of unknown types
                logger.LogDebug("Ignoring frame of unknown type {ft}", (byte)frame.FrameType);
                break;
        }
    }

    private async Task SendServerPrefaceAsync(CancellationToken cancellationToken)
    {
        var settingsFrame = new Http2Frame { FrameType = Http2FrameType.SETTINGS };
        Http2FrameSETTINGSItem[] settings =
        [
            new() { Identifier = Http2FrameSettings.SETTINGS_HEADER_TABLE_SIZE, Value = options.HeaderTableSize },
            new() { Identifier = Http2FrameSettings.SETTINGS_ENABLE_PUSH, Value = 0 },
            new() { Identifier = Http2FrameSettings.SETTINGS_MAX_CONCURRENT_STREAMS, Value = options.MaxConcurrentStreams },
            new() { Identifier = Http2FrameSettings.SETTINGS_INITIAL_WINDOW_SIZE, Value = options.InitialWindowSize },
            new() { Identifier = Http2FrameSettings.SETTINGS_MAX_FRAME_SIZE, Value = options.MaxFrameSize },
            new() { Identifier = Http2FrameSettings.SETTINGS_MAX_HEADER_LIST_SIZE, Value = options.MaxHeaderListSize },
        ];

        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + settings.Length * 6);
        try
        {
            int length = Http2FrameWriter.SerializeSETTINGSFrame(settingsFrame, settings, buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Writes raw, already serialized frames. All writers share this lock so frames from different streams never interleave.
    /// </summary>
    private async Task WriteFrameAsync(ReadOnlyMemory<byte> frames, CancellationToken cancellationToken)
    {
        if (connectionClosed)
        {
            throw new Http2StreamResetException(0, Http2ErrorCode.CANCEL);
        }

        await writeLock.WaitAsync(cancellationToken);
        try
        {
            await protocolHandlerContext.Stream.WriteAsync(frames, cancellationToken);
            await protocolHandlerContext.Stream.FlushAsync(cancellationToken);
        }
        finally
        {
            writeLock.Release();
        }
    }

    private async Task SendRstStreamAsync(uint streamId, Http2ErrorCode errorCode, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + 4);
        try
        {
            int length = Http2FrameWriter.SerializeRST_STREAMFrame(streamId, errorCode, buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task SendWindowUpdateAsync(uint streamId, uint increment, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + 4);
        try
        {
            int length = Http2FrameWriter.SerializeWINDOW_UPDATEFrame(streamId, increment, buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Sends GOAWAY (RFC 9113 6.8) and marks the connection as closed. Never throws, the connection is going away anyway.
    /// </summary>
    private async Task TrySendGoAwayAsync(Http2ErrorCode errorCode, CancellationToken cancellationToken)
    {
        if (goAwaySent)
        {
            return;
        }

        goAwaySent = true;

        var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + 8);
        try
        {
            int length = Http2FrameWriter.SerializeGOAWAYFrame(lastClientStreamId, errorCode, buffer);
            await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to send GOAWAY");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            Shutdown();
        }
    }

    /// <summary>
    /// Marks the connection as unusable, wakes up blocked writers and fails every open request body.
    /// </summary>
    private void Shutdown()
    {
        connectionClosed = true;

        foreach (var stream in streams.Values)
        {
            CloseStream(stream, Http2ErrorCode.CANCEL);
        }

        lock (flowControlLock)
        {
            SignalWindowUpdated();
        }
    }

    /// <summary>
    /// Handles a stream error (RFC 9113 5.4.2): RST_STREAM is sent unless the stream is still idle, and the stream is closed.
    /// </summary>
    private async Task HandleStreamErrorAsync(Http2StreamException ex, CancellationToken cancellationToken)
    {
        logger.LogWarning("HTTP/2 stream {sid} error {code}: {message}", ex.StreamId, ex.ErrorCode, ex.Message);

        if (streams.TryGetValue(ex.StreamId, out var stream))
        {
            CloseStream(stream, ex.ErrorCode);
        }

        if (ex.StreamId != 0 && ex.StreamId <= lastClientStreamId)
        {
            // RFC 9113 6.4: RST_STREAM must not be sent for an idle stream
            await SendRstStreamAsync(ex.StreamId, ex.ErrorCode, cancellationToken);
        }
    }

    private Http2Stream? GetStream(uint streamId)
    {
        return streams.TryGetValue(streamId, out var stream) ? stream : null;
    }

    private bool IsIdleStream(uint streamId)
    {
        // client streams are odd and opened in increasing order, anything above the last one seen has never been used
        return streamId > lastClientStreamId;
    }

    /// <summary>
    /// Removes a stream from the connection. When <paramref name="resetErrorCode"/> is set the stream was aborted and pending
    /// writers and body readers are failed, otherwise it completed normally.
    /// </summary>
    private void CloseStream(Http2Stream stream, Http2ErrorCode? resetErrorCode)
    {
        streams.TryRemove(stream.StreamId, out _);
        streamsByBody.TryRemove(stream.Body, out _);

        lock (flowControlLock)
        {
            stream.State = Http2StreamStates.CLOSED;
            if (resetErrorCode.HasValue && !stream.IsReset)
            {
                stream.ResetErrorCode = resetErrorCode;
            }

            SignalWindowUpdated(); // wakes up writers blocked on this stream so they notice the reset
        }

        stream.CompleteBody(resetErrorCode.HasValue ? new Http2StreamResetException(stream.StreamId, resetErrorCode.Value) : null);
    }

    private void SignalWindowUpdated()
    {
        // called under flowControlLock, every waiter captured the previous task and gets released by completing it
        var previous = windowUpdated;
        windowUpdated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        previous.TrySetResult();
    }
}
