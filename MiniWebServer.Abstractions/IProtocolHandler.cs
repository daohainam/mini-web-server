using MiniWebServer.Abstractions.Http;
using static MiniWebServer.Abstractions.ProtocolHandlerStates;

namespace MiniWebServer.Abstractions
{
    public interface IProtocolHandler
    {
        int ProtocolVersion { get; }
        BuildRequestStates ReadRequest(Span<byte> buffer, IHttpRequestBuilder httpWebRequestBuilder, ProtocolHandlerData data, out int bytesProcessed);
        WriteResponseStates WriteResponse(Span<byte> buffer, HttpResponse response, ProtocolHandlerData protocolHandlerData, out int bytesProcessed);
        void Reset(ProtocolHandlerData data);
    }
}
