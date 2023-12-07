using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class ServerWebSocket(System.Net.WebSockets.WebSocket webSocket, IMiniAppRequestContext context) : System.Net.WebSockets.WebSocket
    {
        public override WebSocketCloseStatus? CloseStatus => webSocket.CloseStatus;

        public override string? CloseStatusDescription => webSocket.CloseStatusDescription;

        public override WebSocketState State => webSocket.State;

        public override string? SubProtocol => webSocket.SubProtocol;

        public override void Abort()
        {
            webSocket.Abort();
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            return webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            return webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override void Dispose()
        {
            webSocket.Dispose();
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return await webSocket.ReceiveAsync(buffer, cancellationToken);
        }

        public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }
    }
}
