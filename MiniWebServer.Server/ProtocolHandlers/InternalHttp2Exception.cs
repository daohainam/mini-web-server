namespace MiniWebServer.Server.ProtocolHandlers
{
    [Serializable]
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