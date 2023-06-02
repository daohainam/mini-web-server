using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public abstract class MiniContent: IHttpContent
    {
        public abstract Abstractions.Http.HttpHeaders Headers { get; }
        public abstract Task<long> WriteToAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken);
        public abstract long ContentLength { get; }
    }
}
