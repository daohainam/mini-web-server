using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.ResponseCompression
{
    internal class GzipCompressionProvider
    {
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

        public Stream CreateStream(Stream stream) => new GZipStream(stream, CompressionLevel);
    }
}
