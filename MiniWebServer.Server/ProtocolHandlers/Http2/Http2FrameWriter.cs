using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class Http2FrameWriter
    {
        internal static Task<bool> SerializeHeaderFrames(uint streamId, IHttpResponse response, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}