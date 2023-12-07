using MiniWebServer.MiniApp;
using System.Text;

namespace MiniWebServer.HstsMiddleware
{
    public class HstsMiddleware(HstsOptions options) : IMiddleware
    {
        public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
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