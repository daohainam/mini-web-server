using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Content;
using System.Text;

namespace MiniWebServer.HstsMiddleware
{
    public class HstsMiddleware : IMiddleware
    {
        private readonly HstsOptions options;

        public HstsMiddleware(HstsOptions options)
        {
            this.options = options;
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new();
            sb.Append("max-age=");
            sb.Append(options.MaxAge);
            if (options.IncludeSubDomains)
            {
                sb.Append("; includeSubDomains");
            }
            if (options.Preload)
            {
                sb.Append("; preload");
            }

            context.Response.Headers.AddOrUpdate("Strict-Transport-Security", sb.ToString());

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}