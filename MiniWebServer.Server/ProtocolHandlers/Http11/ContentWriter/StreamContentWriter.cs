using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentWriter;

internal class StreamContentWriter(Stream stream) : IContentWriter
{
    private readonly Stream stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public void Complete()
    {
    }

    public void Write(ReadOnlySpan<byte> value)
    {
        stream.Write(value);
    }
}
