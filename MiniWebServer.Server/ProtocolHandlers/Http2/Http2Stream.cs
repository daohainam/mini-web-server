using System.IO.Pipelines;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Per-stream state of a client-initiated stream (RFC 9113 5.1), including both flow-control windows (6.9).
/// Window fields are only touched while holding the connection's flow-control lock.
/// </summary>
internal sealed class Http2Stream(uint streamId, long sendWindow, long receiveWindow)
{
    // the writer side is fed by the connection's read loop, the reader side is handed to the application as the request body
    private static readonly PipeOptions BodyPipeOptions = new(pauseWriterThreshold: 0, resumeWriterThreshold: 0, useSynchronizationContext: false);

    private bool bodyCompleted;

    public uint StreamId { get; } = streamId;
    public Http2StreamStates State { get; set; } = Http2StreamStates.IDLE;

    public List<HPACKHeader> Headers { get; set; } = [];
    public Pipe Body { get; } = new(BodyPipeOptions);

    /// <summary>
    /// Value of the request's content-length header, null when absent.
    /// </summary>
    public long? DeclaredContentLength { get; set; }
    public long ReceivedBodyBytes { get; set; }

    public long SendWindow { get; set; } = sendWindow;
    public long ReceiveWindow { get; set; } = receiveWindow;

    /// <summary>
    /// Bytes consumed from the receive window since the last WINDOW_UPDATE we sent for this stream.
    /// </summary>
    public long UnacknowledgedReceivedBytes { get; set; }

    public bool RemoteClosed => State is Http2StreamStates.HALF_CLOSED_REMOTE or Http2StreamStates.CLOSED;
    public bool LocalClosed => State is Http2StreamStates.HALF_CLOSED_LOCAL or Http2StreamStates.CLOSED;

    /// <summary>
    /// Set when the stream was reset (by either side) or the connection is gone, pending writers must give up.
    /// </summary>
    public Http2ErrorCode? ResetErrorCode { get; set; }
    public bool IsReset => ResetErrorCode.HasValue;

    /// <summary>
    /// A header block that is decoded only to keep HPACK state in sync, the stream itself is rejected with this error afterwards.
    /// </summary>
    public Http2ErrorCode? RejectErrorCode { get; set; }

    public bool TrailersExpected { get; set; }

    public void CloseRemote()
    {
        State = State switch
        {
            Http2StreamStates.HALF_CLOSED_LOCAL => Http2StreamStates.CLOSED,
            Http2StreamStates.CLOSED => Http2StreamStates.CLOSED,
            _ => Http2StreamStates.HALF_CLOSED_REMOTE
        };

        CompleteBody(null);
    }

    public void CloseLocal()
    {
        State = State switch
        {
            Http2StreamStates.HALF_CLOSED_REMOTE => Http2StreamStates.CLOSED,
            Http2StreamStates.CLOSED => Http2StreamStates.CLOSED,
            _ => Http2StreamStates.HALF_CLOSED_LOCAL
        };
    }

    public void CompleteBody(Exception? exception)
    {
        if (bodyCompleted)
        {
            return;
        }

        bodyCompleted = true;
        Body.Writer.Complete(exception);
    }
}
