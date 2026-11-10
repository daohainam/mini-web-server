namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public class Http2Exception : Exception
{
    public Http2Exception()
    {
    }

    public Http2Exception(string? message) : base(message)
    {
    }

    public Http2Exception(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// A connection error (RFC 9113 5.4.1): the connection is no longer usable, a GOAWAY frame carrying <see cref="ErrorCode"/> is sent and the connection is closed.
/// </summary>
public sealed class Http2ConnectionException(Http2ErrorCode errorCode, string? message) : Http2Exception(message)
{
    public Http2ErrorCode ErrorCode { get; } = errorCode;
}

/// <summary>
/// A stream error (RFC 9113 5.4.2): only the given stream is affected, a RST_STREAM frame carrying <see cref="ErrorCode"/> is sent.
/// </summary>
public sealed class Http2StreamException(uint streamId, Http2ErrorCode errorCode, string? message) : Http2Exception(message)
{
    public uint StreamId { get; } = streamId;
    public Http2ErrorCode ErrorCode { get; } = errorCode;
}

/// <summary>
/// Thrown to a response writer when the stream it writes to was reset by the peer or the connection went away.
/// </summary>
public sealed class Http2StreamResetException(uint streamId, Http2ErrorCode errorCode) : Http2Exception($"Stream {streamId} was reset: {errorCode}")
{
    public uint StreamId { get; } = streamId;
    public Http2ErrorCode ErrorCode { get; } = errorCode;
}
