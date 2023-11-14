using MiniWebServer.Abstractions;
using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentWriter
{
    internal class ByteBufferContentWriter : IContentWriter
    {
        private readonly IBufferWriter<byte> buffer;

        public ByteBufferContentWriter(IBufferWriter<byte> buffer)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Complete()
        {
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            buffer.Write(value);
        }
    }
}
