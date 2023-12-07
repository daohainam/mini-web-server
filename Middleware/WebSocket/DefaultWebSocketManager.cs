using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class DefaultWebSocketManager(WebSocketOptions options, IMiniAppRequestContext context, ILogger logger) : IWebSocketManager
    {
        private bool? isUpgradeRequest;
        private string? originalNonce;

        public bool IsUpgradeRequest { get {
                return GetIsUpgradeRequest();
            } 
        }

        public async Task<System.Net.WebSockets.WebSocket> AcceptAsync(WebSocketAcceptOptions? acceptOptions = default, CancellationToken cancellationToken = default)
        {
            if (!GetIsUpgradeRequest())
            {
                throw new InvalidOperationException("Not a websocket request");
            }

            if (originalNonce == null)
            {
                throw new InvalidOperationException("Nonce is invalid");
            }

            string upgradeResponse = $"HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: {WebSocketHandshakeHelpers.BuildSecWebSocketAccept(originalNonce)}\r\n\r\n";
            logger.LogDebug("Sending Switching Protocols protocol: {r}", upgradeResponse);

            await context.Response.Stream.WriteAsync(Encoding.UTF8.GetBytes(upgradeResponse), cancellationToken);
            await context.Response.Stream.FlushAsync(cancellationToken);

            var webSocket = System.Net.WebSockets.WebSocket.CreateFromStream(context.Response.Stream, new WebSocketCreationOptions()
            {
                IsServer = true,
                KeepAliveInterval = options.KeepAliveInterval,
                SubProtocol = options.SubProtocol,
                // TODO: we disable Compression for now, will re-enable when we handle Sec-WebSocket-Extensions
                //DangerousDeflateOptions = acceptOptions?.WebSocketDeflateOptions ?? new WebSocketDeflateOptions() // TODO: this property is relatively complicated so I use a hard-coded (default) value for now, but we should parse it in order to work properly
                //{
                //    ServerContextTakeover = false,
                //}
            });
            var serverWebSocket = new ServerWebSocket(webSocket, context);
            return serverWebSocket;
        }

        private bool GetIsUpgradeRequest()
        {
            if (isUpgradeRequest == null)
            {
                isUpgradeRequest = false;

                if (WebSocketHandshakeHelpers.IsUpgradeRequest(context, out originalNonce, logger))
                {
                    isUpgradeRequest = true;
                }
            }

            return isUpgradeRequest.Value;
        }
    }
}
