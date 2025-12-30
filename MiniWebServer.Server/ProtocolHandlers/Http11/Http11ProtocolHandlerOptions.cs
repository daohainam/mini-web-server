using MiniWebServer.Server.Abstractions.Parsers.Http11;

namespace MiniWebServer.Server.ProtocolHandlers.Http11;

public class Http11ProtocolHandlerOptions(
    IHttpComponentParser http11Parser,
    int readBufferSize,
    int writeBufferSize,
    int textBufferSize)
{
    public IHttpComponentParser Http11Parser { get; } = http11Parser ?? throw new ArgumentNullException(nameof(http11Parser));
    public int ReadBufferSize { get; } = readBufferSize;
    public int WriteBufferSize { get; } = writeBufferSize;
    public int TextBufferSize { get; } = textBufferSize;
}
