using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using System.Buffers.Text;
using System.Text;

namespace MiniWebServer.WebSocket
{
    // https://datatracker.ietf.org/doc/html/rfc6455
    public class WebSocketMiddleware: IMiddleware
    {
        private WebSocketOptions options;
        private readonly ILogger<WebSocketMiddleware> logger;

        public WebSocketMiddleware(WebSocketOptions options, ILogger<WebSocketMiddleware> logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? NullLogger<WebSocketMiddleware>.Instance;
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsUpgradeRequest(context, out string? originalNonce) && originalNonce != null)
                {
                    // now we are ready to upgrade the connection to websocket

                    context.Response.StatusCode = Abstractions.HttpResponseCodes.SwitchingProtocols;
                    context.Response.Headers.Connection = "Upgrade";
                    context.Response.Headers.SecWebSocketAccept = WebSocketHandshakeHelpers.BuildSecWebSocketAccept(originalNonce);
                }
            } catch (Exception ex) {
                logger.LogError(ex, "Error checking WebSocket request");

                // it is actually not necessary to send Bad Request, according to RFC6455 we can keep the connection open and continue serving HTTP requests
                context.Response.StatusCode = Abstractions.HttpResponseCodes.BadRequest; 
                return;
            }

            await next.InvokeAsync(context, cancellationToken);
        }

        private bool IsUpgradeRequest(IMiniAppContext context, out string? originalNonce)
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
            if (context.Request.Method == Abstractions.Http.HttpMethod.Get)
            {
                if ("websocket".Equals(context.Request.Headers.Upgrade, StringComparison.OrdinalIgnoreCase)
                    && "Upgrade".Equals(context.Request.Headers.Connection, StringComparison.OrdinalIgnoreCase))
                {
                    var secWebSocketKey = context.Request.Headers.SecWebSocketKey;
                    if (!string.IsNullOrWhiteSpace(secWebSocketKey))
                    {
                        try
                        {
                            var bytes = Convert.FromBase64String(secWebSocketKey);
                            if (bytes.Length != 16)
                            {
                                throw new FormatException("Sec-WebSocket-Key original value length must be 16");
                            }
                            foreach (var b in bytes)
                            {
                                if (b < 32 || b > 127)
                                {
                                    throw new FormatException("Sec-WebSocket-Key contains invalid characters");
                                }
                            }
                            originalNonce = Encoding.UTF8.GetString(bytes);
                        }
                        catch {
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
