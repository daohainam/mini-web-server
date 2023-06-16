using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentWriter
{
    internal class ByteBufferContentWriter : IContentWriter
    {
        private readonly IBufferWriter<byte> buffer;

        public ByteBufferContentWriter(IBufferWriter<byte> buffer) {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            buffer.Write(value);
        }
    }
}
