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
        Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken);
    }
}
