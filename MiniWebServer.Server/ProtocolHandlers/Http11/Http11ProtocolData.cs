//using MiniWebServer.Abstractions;
//using System;
//using System.Collections.Generic;
//using System.IO.Pipelines;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using HttpMethod = global::MiniWebServer.Abstractions.Http.HttpMethod;
//using HttpRequestHeaders = global::MiniWebServer.Abstractions.Http.HttpRequestHeaders;

//namespace MiniWebServer.Server.ProtocolHandlers.Http11
//{
//    public class Http11ProtocolData
//    {
//        public Http11ProtocolData()
//        {
//            BodyPipeline = new Pipe();
//        }

//        public Http11RequestMessageParts CurrentReadingPart { get; set; } = Http11RequestMessageParts.RequestLine;
//        public Http11ResponseMessageParts CurrentWritingPart { get; set; } = Http11ResponseMessageParts.StatusLine;
//        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;
//        public long ContentLength { get; internal set; } = 0;
//        public string[] TransferEncoding { get; internal set; } = Array.Empty<string>();
//        public Memory<byte> ResponseHeaderBuffer { get; internal set; } = Array.Empty<byte>().AsMemory();
//        public int ResponseHeaderBufferIndex { get; internal set; } = 0;
//        public int ResponseBodyContentIndex { get; internal set; } = 0;
//        public long CurrentRequestBodyBytes { get; internal set; } = 0;
//        public HttpRequestHeaders RequestHeaders { get; internal set; } = new();
//        public Pipe BodyPipeline { get; internal set; }
//    }
//}
