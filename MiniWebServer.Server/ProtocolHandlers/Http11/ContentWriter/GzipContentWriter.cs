using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11.ContentEncoding
{
    public class GzipContentWriter : IContentWriter, IDisposable
    {
        private IBufferWriter<byte> parentWriter;
        private readonly GZipStream gzipStream;
        private readonly MemoryStream buffer;

        private bool disposedValue;

        public GzipContentWriter(IBufferWriter<byte> parentWriter)
        {
            this.parentWriter = parentWriter;
            
            // todo: add IDisposable and ArrayPool support
            buffer = new MemoryStream(16 * 1024);
            gzipStream = new(buffer, CompressionLevel.Optimal);
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            gzipStream.Write(value);
            gzipStream.Flush();
            var mbuffer = buffer.GetBuffer();
            parentWriter.Write(mbuffer.AsSpan(0, (int)buffer.Length));
            buffer.SetLength(0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GzipContentWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
