using MiniWebServer.Abstractions.Http;
using System.Buffers;
using System.IO.Pipelines;

namespace MiniWebServer.Abstractions
{
    public interface IProtocolHandler
    {
        int ProtocolVersion { get; }
        Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder httpWebRequestBuilder, CancellationToken cancellationToken);
        Task<bool> WriteResponseAsync(IBufferWriter<byte> writer, HttpResponse response, CancellationToken cancellationToken);
        void Reset();
    }
}
