namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// A write-only stream handed to response content: every write becomes flow-controlled DATA frames on one HTTP/2 stream.
/// The final empty DATA frame with END_STREAM is sent by the protocol handler once the content is written.
/// </summary>
internal sealed class Http2ResponseBodyStream(Http2ProtocolHandler handler, Http2Stream stream, CancellationToken cancellationToken) : Stream
{
    private readonly CancellationToken responseCancellationToken = cancellationToken;

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        // DATA frames are flushed as they are written
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        // content writers that use the synchronous API block until the frames are on the wire
        handler.WriteDataAsync(stream, buffer.ToArray(), endStream: false, responseCancellationToken).GetAwaiter().GetResult();
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(responseCancellationToken, cancellationToken);
        await handler.WriteDataAsync(stream, buffer, endStream: false, linked.Token);
    }
}
