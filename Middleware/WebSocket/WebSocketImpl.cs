using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class WebSocketImpl : IWebSocket
    {
        private readonly Stream inStream;
        private readonly Stream outStream;

        public WebSocketImpl(Stream inStream, Stream outStream)
        {
            this.inStream = inStream ?? throw new ArgumentNullException(nameof(inStream));
            this.outStream = outStream ?? throw new ArgumentNullException(nameof(outStream));
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(Memory<byte> bytes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(Memory<byte> bytes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
