using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System.Buffers.Text;
using System.Text;

namespace MiniWebServer.WebSocket
{
    // https://datatracker.ietf.org/doc/html/rfc6455
    public class WebSocketMiddleware(WebSocketOptions options, ILogger<WebSocketMiddleware> logger) : IMiddleware
    {
        private readonly WebSocketOptions options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger<WebSocketMiddleware> logger = logger ?? NullLogger<WebSocketMiddleware>.Instance;

        public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            if (options.RequestMatcher(context))
            {
                try
                {
                    context.WebSockets = new DefaultWebSocketManager(options, context, logger);
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
    }
}
