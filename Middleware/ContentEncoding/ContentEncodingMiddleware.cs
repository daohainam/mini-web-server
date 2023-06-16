using MiniWebServer.MiniApp;

namespace MiniWebServer.ContentEncoding
{
    public class ContentEncodingMiddleware : IMiddleware
    {
        // we will compress response content and update Content-Length, Content-MD5, Content-Encoding

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            await next.InvokeAsync(context, cancellationToken);
        }
    }
}