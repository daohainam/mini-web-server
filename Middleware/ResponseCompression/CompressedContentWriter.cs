using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.ResponseCompression
{
    internal class CompressedContentWriter : IContentWriter
    {
        private readonly IContentWriter parent;

        public CompressedContentWriter(IContentWriter parent) { 
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public void Complete()
        {
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            parent.Write(value);
        }
    }
}
