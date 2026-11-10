namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// Thrown when a header block cannot be decoded. HTTP/2 maps it to a connection error of type COMPRESSION_ERROR (RFC 9113 4.3).
/// </summary>
public class HPACKDecodingException(string message) : Exception(message)
{
}

/// <summary>
/// Thrown when a header block decodes fine but exceeds the limits the receiver is willing to accept (RFC 9113 10.5.1).
/// </summary>
public sealed class HPACKHeaderListTooLargeException(string message) : HPACKDecodingException(message)
{
}
