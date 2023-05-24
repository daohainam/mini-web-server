using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using System.Buffers;
using System.IO.Pipelines;

namespace MiniWebServer.Server.Abstractions
{
    public interface IProtocolHandler
    {
        int ProtocolVersion { get; }
        Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder httpWebRequestBuilder, CancellationToken cancellationToken);
        Task<bool> WriteResponseAsync(IBufferWriter<byte> writer, IHttpResponse response, CancellationToken cancellationToken);
        void Reset();
    }
}
