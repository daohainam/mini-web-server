using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentWriter
{
    internal class StreamContentWriter : IContentWriter
    {
        private readonly Stream stream;

        public StreamContentWriter(Stream stream) {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void Complete()
        {
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            stream.Write(value);
        }
    }
}
