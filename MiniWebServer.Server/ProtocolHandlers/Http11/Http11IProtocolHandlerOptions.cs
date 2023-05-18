using MiniWebServer.Abstractions.HttpParser.Http11;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11IProtocolHandlerOptions
    {
        public Http11IProtocolHandlerOptions(
            IHttp11Parser http11Parser, 
            int readBufferSize, 
            int writeBufferSize, 
            int textBufferSize)
        {
            Http11Parser = http11Parser ?? throw new ArgumentNullException(nameof(http11Parser));
            ReadBufferSize = readBufferSize;
            WriteBufferSize = writeBufferSize;
            TextBufferSize = textBufferSize;
        }

        public IHttp11Parser Http11Parser { get; }
        public int ReadBufferSize { get; }
        public int WriteBufferSize { get; }
        public int TextBufferSize { get; }
    }
}
