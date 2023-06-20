using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Http;
using System.Buffers;
using System.IO.Pipelines;

namespace MiniWebServer.Server.Abstractions
{
    public interface IProtocolHandler
    {
        int ProtocolVersion { get; }
        /// <summary>
        /// Read request line and headers to requestBuilder, since a body can be very big, we will process it later when an app requests
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="requestBuilder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken);
        Task<bool> WriteResponseAsync(PipeWriter writer, IHttpResponse response, CancellationToken cancellationToken);
        Task<bool> WriteResponseAsync(Stream stream, IHttpResponse response, CancellationToken cancellationToken);
        void Reset();
        Task ReadBodyAsync(PipeReader reader, IHttpRequest requestBuilder, CancellationToken cancellationToken);
    }
}
