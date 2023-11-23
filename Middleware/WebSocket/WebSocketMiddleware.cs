using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System.Buffers.Text;
using System.Text;

namespace MiniWebServer.WebSocket
{
    // https://datatracker.ietf.org/doc/html/rfc6455
    public class WebSocketMiddleware(WebSocketOptions options, ILogger<WebSocketMiddleware> logger, DefaultWebSocketAppBuilder webSocketAppBuilder) : IMiddleware
    {
        private readonly WebSocketOptions options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger<WebSocketMiddleware> logger = logger ?? NullLogger<WebSocketMiddleware>.Instance;
        private readonly DefaultWebSocketAppBuilder webSocketAppBuilder = webSocketAppBuilder ?? throw new ArgumentNullException(nameof(webSocketAppBuilder));

        public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            if (options.RequestMatcher(context))
            {
                try
                {
                    var handler = webSocketAppBuilder.FindHandler(context.Request.Url);

                    if (IsUpgradeRequest(context, out string? originalNonce) && originalNonce != null && handler != null)
                    {
                        // now we are ready to upgrade the connection to websocket
                        logger.LogInformation("Connection ready to upgrade to WebSocket");

                        context.Response.StatusCode = MiniWebServer.Abstractions.HttpResponseCodes.SwitchingProtocols;
                        context.Response.Headers.Connection = "Upgrade";
                        context.Response.Headers.SecWebSocketAccept = WebSocketHandshakeHelpers.BuildSecWebSocketAccept(originalNonce);

                        context.WebSockets = new DefaultWebSocketManager
                        {
                            IsUpgradeRequest = true,
                            Handler = handler
                        };
                    }
                    else
                    {
                        context.Response.StatusCode = MiniWebServer.Abstractions.HttpResponseCodes.BadRequest;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error checking WebSocket request");

                    // it is actually not necessary to send Bad Request, according to RFC6455 we can keep the connection open and continue serving HTTP requests
                    context.Response.StatusCode = MiniWebServer.Abstractions.HttpResponseCodes.BadRequest;
                    return;
                }
            }
            else
            {
                await next.InvokeAsync(context, cancellationToken);
            }
        }

        private bool IsUpgradeRequest(IMiniAppRequestContext context, out string? originalNonce)
        {
            /*
             * check if this is a upgrade request
             * an upgrade request must be a GET request and has following headers:
             * - Upgrade: websocket
             * - Connection: Upgrade
             * - Sec-WebSocket-Key: <key> // a base64 encoded string from a randomly generated 16-ASCII-character-length string
             * - Sec-WebSocket-Version: <version>
             * 
             * the request can also contains following headers:
             *  - Sec-WebSocket-Protocol: <protocol>
             *  - Sec-WebSocket-Accept
             */

            // TODO: to prevent DDOS, we should handle re-handshakes, and should not allow multiple handshakes from same IP
            if (context.Request.Method == MiniWebServer.Abstractions.Http.HttpMethod.Get)
            {
                if ("websocket".Equals(context.Request.Headers.Upgrade, StringComparison.OrdinalIgnoreCase)
                    && "Upgrade".Equals(context.Request.Headers.Connection, StringComparison.OrdinalIgnoreCase)
                    && "13".Equals(context.Request.Headers.SecWebSocketVersion) // current version of WebSocket protocol, I don't think they will have a newer one soon
                    )
                {
                    var secWebSocketKey = context.Request.Headers.SecWebSocketKey;
                    if (!string.IsNullOrWhiteSpace(secWebSocketKey))
                    {
                        try
                        {
                            /*
                            var bytes = Convert.FromBase64String(secWebSocketKey);
                            if (bytes.Length != 16)
                            {
                                throw new FormatException("Sec-WebSocket-Key original value length must be 16");
                            }
                            */
                            originalNonce = secWebSocketKey;
                        }
                        catch
                        {
                            logger.LogError("Invalid Sec-WebSocket-Key value: {v}", secWebSocketKey);
                            throw;
                        }

                        return originalNonce != null;
                    }
                }
            }

            originalNonce = null;
            return false;
        }


    }
}
