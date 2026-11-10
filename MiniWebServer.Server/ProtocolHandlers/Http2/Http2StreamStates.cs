namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Stream states, RFC 9113 5.1. Only the states a server without server push can reach are used.
/// </summary>
internal enum Http2StreamStates
{
    IDLE,
    OPEN,
    HALF_CLOSED_REMOTE,
    HALF_CLOSED_LOCAL,
    CLOSED
}
