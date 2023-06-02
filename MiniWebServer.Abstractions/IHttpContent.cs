using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface IHttpContent
    {
        HttpHeaders Headers { get; }
        long ContentLength { get; }
        Task<long> WriteToAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken);
    }
}
