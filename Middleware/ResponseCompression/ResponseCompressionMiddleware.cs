using MiniWebServer.MiniApp;

namespace MiniWebServer.ResponseCompression
{
    public class ResponseCompressionMiddleware : IMiddleware 
    {
        // I'm not going to finish this yet, compressing large files will apinfully affect server's performance,
        // so I will implement chunked encoding support first and we only compress small chunks instead of one large response

        private ResponseCompressionOptions options;

        public ResponseCompressionMiddleware(ResponseCompressionOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        // we will compress response content and update Content-Length, Content-MD5, Content-Encoding

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            await next.InvokeAsync(context, cancellationToken);

            var requestedContentEncoding = context.Request.Headers.AcceptEncoding;
            if (requestedContentEncoding.Length > 0)
            {
                if (requestedContentEncoding.Contains("gzip"))
                {
                    context.Response.Headers.Add("ContentEncodingMiddleware", "gzip");

                    var compressionProvider = new GzipCompressionProvider();
                }
            }
        }
    }
}