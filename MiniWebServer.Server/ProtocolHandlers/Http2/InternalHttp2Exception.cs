using System;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    /// <summary>
    /// Exception thrown for internal HTTP/2 protocol errors during frame serialization
    /// </summary>
    internal class InternalHttp2Exception : Exception
    {
        public InternalHttp2Exception()
        {
        }

        public InternalHttp2Exception(string? message) : base(message)
        {
        }

        public InternalHttp2Exception(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
