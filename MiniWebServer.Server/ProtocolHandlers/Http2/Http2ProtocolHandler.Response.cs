using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System.Buffers;
using System.Globalization;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    /// <summary>
    /// Sends the response on the stream the request arrived on: one HEADERS block, DATA frames under flow control, END_STREAM.
    /// </summary>
    public async Task<bool> WriteResponseAsync(IHttpRequest? request, IHttpResponse response, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (request == null || !streamsByBody.TryGetValue(request.BodyPipeline, out var stream))
        {
            logger.LogError("No HTTP/2 stream is associated with this response");
            return false;
        }

        try
        {
            if (stream.IsReset || stream.LocalClosed)
            {
                return true; // the peer gave up on this stream, there is nobody to answer
            }

            var headers = BuildResponseHeaders(response);

            // RFC 9110 6.4.1 / 9.3.2: HEAD, 1xx, 204 and 304 responses never carry content
            int status = (int)response.StatusCode;
            bool bodyAllowed = !(request.Method == HttpMethod.Head || status is >= 100 and < 200 || status is 204 or 304);
            bool hasBody = bodyAllowed && response.Content is not EmptyContent;

            await WriteHeaderBlockAsync(stream, headers, endStream: !hasBody, cancellationToken);

            if (hasBody)
            {
                await using var bodyStream = new Http2ResponseBodyStream(this, stream, cancellationToken);
                await response.Content.WriteToAsync(bodyStream, cancellationToken);
                await WriteDataAsync(stream, ReadOnlyMemory<byte>.Empty, endStream: true, cancellationToken);
            }

            await FinishResponseAsync(stream, cancellationToken);

            return true;
        }
        catch (Http2StreamResetException ex)
        {
            logger.LogDebug("Response on stream {sid} abandoned: {code}", stream.StreamId, ex.ErrorCode);
            return true;
        }
    }

    private static List<HPACKHeader> BuildResponseHeaders(IHttpResponse response)
    {
        var headers = new List<HPACKHeader>
        {
            // RFC 9113 8.3.2: :status is the only response pseudo-header and must come first
            new(HPACKStaticTable.STATUS, ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture))
        };

        foreach (var header in response.Content.Headers)
        {
            response.Headers.AddOrUpdate(header.Key, header.Value);
        }

        foreach (var header in response.Headers)
        {
            string name = header.Key.ToLowerInvariant(); // RFC 9113 8.2.1: field names must be lowercase
            if (name.Length == 0 || ConnectionSpecificHeaders.Contains(name))
            {
                continue; // RFC 9113 8.2.2: connection-specific fields are dropped
            }

            foreach (var value in header.Value)
            {
                headers.Add(new HPACKHeader(name, SanitizeFieldValue(value)));
            }
        }

        foreach (var cookie in response.Cookies)
        {
            headers.Add(new HPACKHeader("set-cookie", SanitizeFieldValue(cookie.Value.ToString())));
        }

        return headers;
    }

    private static string SanitizeFieldValue(string value)
    {
        // RFC 9113 8.2.1: NUL, CR and LF are not allowed in field values
        if (value.AsSpan().IndexOfAny('\0', '\r', '\n') < 0)
        {
            return value.Trim(' ', '\t');
        }

        return value.Replace("\0", "").Replace("\r", "").Replace("\n", "").Trim(' ', '\t');
    }

    private async Task WriteHeaderBlockAsync(Http2Stream stream, List<HPACKHeader> headers, bool endStream, CancellationToken cancellationToken)
    {
        if (connectionClosed)
        {
            throw new Http2StreamResetException(stream.StreamId, Http2ErrorCode.CANCEL);
        }

        var block = new ArrayBufferWriter<byte>();
        var frames = new ArrayBufferWriter<byte>();

        // the encoder carries per-connection state, and the HEADERS/CONTINUATION sequence must not be interleaved with other frames
        await writeLock.WaitAsync(cancellationToken);
        try
        {
            encoder.Encode(block, headers);

            int maxFrameSize;
            lock (flowControlLock)
            {
                maxFrameSize = (int)peerMaxFrameSize;
            }

            Http2FrameWriter.WriteHeaderBlock(frames, stream.StreamId, block.WrittenSpan, endStream, maxFrameSize);

            await protocolHandlerContext.Stream.WriteAsync(frames.WrittenMemory, cancellationToken);
            await protocolHandlerContext.Stream.FlushAsync(cancellationToken);
        }
        finally
        {
            writeLock.Release();
        }
    }

    /// <summary>
    /// Sends <paramref name="data"/> as DATA frames, waiting for WINDOW_UPDATE whenever the stream or connection send window is exhausted (RFC 9113 6.9).
    /// </summary>
    internal async Task WriteDataAsync(Http2Stream stream, ReadOnlyMemory<byte> data, bool endStream, CancellationToken cancellationToken)
    {
        int offset = 0;
        do
        {
            int chunk;
            while (true)
            {
                Task waiter;
                lock (flowControlLock)
                {
                    if (stream.IsReset)
                    {
                        throw new Http2StreamResetException(stream.StreamId, stream.ResetErrorCode!.Value);
                    }

                    if (connectionClosed)
                    {
                        throw new Http2StreamResetException(stream.StreamId, Http2ErrorCode.CANCEL);
                    }

                    long available = Math.Min(Math.Min(stream.SendWindow, connectionSendWindow), peerMaxFrameSize);
                    chunk = (int)Math.Min(available, data.Length - offset);
                    if (chunk > 0 || data.Length == 0)
                    {
                        stream.SendWindow -= chunk;
                        connectionSendWindow -= chunk;
                        break;
                    }

                    waiter = windowUpdated.Task; // captured under the lock, so an update between here and the await is not lost
                }

                await waiter.WaitAsync(cancellationToken);
            }

            bool last = offset + chunk >= data.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(Http2FrameWriter.FrameHeaderLength + chunk);
            try
            {
                int length = Http2FrameWriter.SerializeDATAFrame(stream.StreamId, data.Span.Slice(offset, chunk), endStream && last, buffer);
                await WriteFrameAsync(buffer.AsMemory(0, length), cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            offset += chunk;
        } while (offset < data.Length);
    }

    private async Task FinishResponseAsync(Http2Stream stream, CancellationToken cancellationToken)
    {
        bool remoteClosed;
        lock (flowControlLock)
        {
            stream.CloseLocal();
            remoteClosed = stream.RemoteClosed;
        }

        if (remoteClosed)
        {
            CloseStream(stream, null);
            return;
        }

        // RFC 9113 8.1: a server that sent a complete response can abort the rest of the request with RST_STREAM NO_ERROR
        CloseStream(stream, null);
        await SendRstStreamAsync(stream.StreamId, Http2ErrorCode.NO_ERROR, cancellationToken);
    }
}
