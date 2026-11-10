using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Globalization;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    private void ProcessHEADERSFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.2
        uint streamId = frame.StreamIdentifier;
        if (streamId == 0)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "HEADERS frames must not use stream identifier 0");
        }

        if ((streamId & 1) == 0)
        {
            // RFC 9113 5.1.1: streams initiated by a client use odd-numbered identifiers
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, $"Client stream identifier {streamId} is not odd");
        }

        if (!Http2FrameReader.TryReadHEADERSFramePayload(frame, payload, out var headersPayload))
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "Invalid padding or priority fields in HEADERS frame");
        }

        // whatever happens to the stream, the header block must be decoded to keep the HPACK dynamic table in sync (RFC 9113 4.3)
        var stream = GetStream(streamId);
        if (stream == null)
        {
            stream = new Http2Stream(streamId, peerInitialWindowSize, options.InitialWindowSize);

            if (!IsIdleStream(streamId))
            {
                // RFC 9113 5.1: frames on a closed stream are a stream error of type STREAM_CLOSED
                stream.State = Http2StreamStates.CLOSED;
                stream.RejectErrorCode = Http2ErrorCode.STREAM_CLOSED;
            }
            else
            {
                lastClientStreamId = streamId;
                stream.State = Http2StreamStates.OPEN;

                if (goAwayReceived || streams.Count >= options.MaxConcurrentStreams)
                {
                    // RFC 9113 5.1.2: exceeding SETTINGS_MAX_CONCURRENT_STREAMS is a stream error of type REFUSED_STREAM
                    stream.RejectErrorCode = Http2ErrorCode.REFUSED_STREAM;
                }
                else
                {
                    streams[streamId] = stream;
                }
            }
        }
        else if (stream.RemoteClosed)
        {
            stream.RejectErrorCode = Http2ErrorCode.STREAM_CLOSED;
        }
        else
        {
            // a second HEADERS frame on an open stream carries trailers (RFC 9113 8.1)
            stream.TrailersExpected = true;
        }

        if (headersPayload.HasPriority && headersPayload.StreamDependency == streamId)
        {
            stream.RejectErrorCode ??= Http2ErrorCode.PROTOCOL_ERROR; // a stream cannot depend on itself
        }

        headerBlockStream = stream;
        headerBlockEndStream = frame.Flags.HasFlag(Http2FrameFlags.END_STREAM);
        headerBlock.Clear();

        AppendHeaderBlockFragment(headersPayload.FieldBlockFragment);

        if (frame.Flags.HasFlag(Http2FrameFlags.END_HEADERS))
        {
            FinishHeaderBlock();
        }
    }

    private void ProcessCONTINUATIONFrame(Http2Frame frame, ReadOnlySequence<byte> payload)
    {
        // RFC 9113 6.10: only valid directly after HEADERS or CONTINUATION without END_HEADERS, on the same stream
        if (headerBlockStream == null || frame.StreamIdentifier != headerBlockStream.StreamId)
        {
            throw new Http2ConnectionException(Http2ErrorCode.PROTOCOL_ERROR, "Unexpected CONTINUATION frame");
        }

        AppendHeaderBlockFragment(payload);

        if (frame.Flags.HasFlag(Http2FrameFlags.END_HEADERS))
        {
            FinishHeaderBlock();
        }
    }

    private void AppendHeaderBlockFragment(ReadOnlySequence<byte> fragment)
    {
        if (headerBlock.WrittenCount + fragment.Length > options.MaxHeaderBlockSize)
        {
            // we cannot decode a block we refuse to buffer, and skipping it would desynchronize HPACK, so the connection has to go
            throw new Http2ConnectionException(Http2ErrorCode.ENHANCE_YOUR_CALM, "Header block too large");
        }

        foreach (var segment in fragment)
        {
            headerBlock.Write(segment.Span);
        }
    }

    private void FinishHeaderBlock()
    {
        var stream = headerBlockStream!;
        bool endStream = headerBlockEndStream;
        headerBlockStream = null;

        var headers = new List<HPACKHeader>();
        try
        {
            decoder.Decode(headerBlock.WrittenSpan, headers);
        }
        catch (HPACKHeaderListTooLargeException)
        {
            // the block was fully decoded so HPACK state is intact, only this stream is refused (RFC 9113 10.5.1)
            stream.RejectErrorCode ??= Http2ErrorCode.ENHANCE_YOUR_CALM;
        }
        catch (HPACKDecodingException ex)
        {
            throw new Http2ConnectionException(Http2ErrorCode.COMPRESSION_ERROR, ex.Message);
        }
        finally
        {
            headerBlock.Clear();
        }

        if (stream.RejectErrorCode.HasValue)
        {
            throw new Http2StreamException(stream.StreamId, stream.RejectErrorCode.Value, "Stream rejected");
        }

        if (stream.TrailersExpected)
        {
            stream.TrailersExpected = false;

            // RFC 9113 8.1: trailers end the stream and must not contain pseudo-header fields
            if (!endStream)
            {
                throw new Http2StreamException(stream.StreamId, Http2ErrorCode.PROTOCOL_ERROR, "Trailers must carry the END_STREAM flag");
            }

            if (headers.Any(h => h.Name.StartsWith(':')))
            {
                throw new Http2StreamException(stream.StreamId, Http2ErrorCode.PROTOCOL_ERROR, "Trailers must not contain pseudo-header fields");
            }

            CompleteRequestBody(stream);
            return;
        }

        stream.Headers = headers;
        stream.DeclaredContentLength = ReadContentLength(stream.StreamId, headers);

        if (endStream)
        {
            CompleteRequestBody(stream);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Stream {sid} request headers complete ({n} fields)", stream.StreamId, headers.Count);
        }

        readyStreams.Enqueue(stream);
    }

    /// <summary>
    /// Marks the remote side of a stream as closed after END_STREAM and checks the body against content-length (RFC 9113 8.1.1).
    /// </summary>
    private void CompleteRequestBody(Http2Stream stream)
    {
        if (stream.DeclaredContentLength is long declared && declared != stream.ReceivedBodyBytes)
        {
            throw new Http2StreamException(stream.StreamId, Http2ErrorCode.PROTOCOL_ERROR, $"Received {stream.ReceivedBodyBytes} body bytes but content-length is {declared}");
        }

        lock (flowControlLock)
        {
            stream.CloseRemote();
        }

        if (stream.State == Http2StreamStates.CLOSED)
        {
            CloseStream(stream, null);
        }
    }

    private static long? ReadContentLength(uint streamId, List<HPACKHeader> headers)
    {
        long? contentLength = null;
        foreach (var header in headers)
        {
            if (header.Name != "content-length")
            {
                continue;
            }

            // RFC 9110 8.6: the value is a non-negative integer, duplicates must agree
            if (!long.TryParse(header.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || (contentLength.HasValue && contentLength.Value != value))
            {
                throw new Http2StreamException(streamId, Http2ErrorCode.PROTOCOL_ERROR, "Invalid content-length header");
            }

            contentLength = value;
        }

        return contentLength;
    }
}
