using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    /* 
     * protocol handler for http2 based on RFC 9113 (https://datatracker.ietf.org/doc/html/rfc9113)
     * 
     * Some problems we must solve before going online:
     * - find a way to switch between HTTP11 and HTTP2 (will it be handled in protocol handler factory in or internally inside HTTP2 protocol handler?)
     * - how do we handle frame buffers effectively? will it need to be shared between threads?
     * - a buffer will be controlled inside or outside protocol handlers?
     * 
     * Note:
     * - use buffer pools 
     * 
     */

    public class Http2ProtocolHandler : IProtocolHandler
    {
        public int ProtocolVersion => 20;
        private Dictionary<uint, Http2Stream> streams = new(); // we don't use concurrent dictionary because we will implement our own sync merchanism

        public Task ReadBodyAsync(PipeReader reader, IHttpRequest requestBuilder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteResponseAsync(IHttpResponse response, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
