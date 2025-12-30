using MiniWebServer.Abstractions;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentWriter;

internal class ByteBufferContentWriter(IBufferWriter<byte> buffer) : IContentWriter
{
    private readonly IBufferWriter<byte> buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

    public void Complete()
    {
    }

    public void Write(ReadOnlySpan<byte> value)
    {
        buffer.Write(value);
    }
}
