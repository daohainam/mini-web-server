using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11ProtocolData
    {
        public Http11ProtocolData()
        {
        }

        public Http11RequestMessageParts CurrentReadingPart { get; set; } = Http11RequestMessageParts.RequestLine;
        public Http11ResponseMessageParts CurrentWritingPart { get; set; } = Http11ResponseMessageParts.StatusLine;
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;
        public bool KeepAlive { get; set; } = true; // Keep-Alive is true by default in HTTP 1.1
        public StringBuilder HeaderStringBuilder { get; } = new StringBuilder();
        public long ContentLength { get; internal set; } = 0;
        public string[] TransferEncoding { get; internal set; } = Array.Empty<string>();
        public Memory<byte> ResponseHeaderBuffer { get; internal set; } = Array.Empty<byte>().AsMemory();
        public int ResponseHeaderBufferIndex { get; internal set; } = 0;
        public int ResponseBodyContentIndex { get; internal set; } = 0;
    }
}
