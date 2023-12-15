using MiniWebServer.Abstractions.Http;
using System.Runtime.Serialization;

namespace MiniWebServer.Abstractions
{
    public class InvalidHeaderException(HttpHeader header) : Exception
    {
        public HttpHeader Header { get; private set; } = header;

    }
}
