using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp.Content;
using MiniWebServer.Server.Abstractions;

namespace MiniWebServer.MiniApp.Web.StaticFileSupport
{
    public class StaticFileCallable : ICallable
    {
        private readonly FileInfo file;
        private readonly ILogger logger;
        private readonly IMimeTypeMapping mimeTypeMapping;

        public StaticFileCallable(FileInfo file, IMimeTypeMapping mimeTypeMapping, ILogger logger)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mimeTypeMapping = mimeTypeMapping ?? throw new ArgumentNullException(nameof(mimeTypeMapping));
        }

        private static bool IsText(string mimeType) => mimeType.StartsWith("text/");

        public async Task Get(IMiniAppContext context, CancellationToken cancellationToken)
        {
            if (file.Exists)
            {
                try
                {
                    string fileExt = file.Extension;
                    string mimeType = mimeTypeMapping.GetMimeMapping(fileExt);

                    if (IsText(mimeType))
                    {
                        using var reader = file.OpenText();
                        var content = await reader.ReadToEndAsync(cancellationToken);

                        context.Response.AddHeader("Content-Type", mimeType);
                        context.Response.AddHeader("Content-Length", content.Length.ToString());
                        context.Response.SetContent(new Content.StringContent(content));
                    }
                    else
                    {
                        using var stream = file.OpenRead();
                        var length = file.Length;
                        var content = new byte[length];

                        stream.Read(content, 0, content.Length);

                        context.Response.AddHeader("Content-Type", mimeType);
                        context.Response.AddHeader("Content-Length", content.Length.ToString());
                        context.Response.SetContent(new Content.ByteArrayContent(content));
                    }

                    context.Response.SetStatus(HttpResponseCodes.OK);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, message: null);
                    context.Response.SetStatus(HttpResponseCodes.InternalServerError);
                }
            }
            else
            {
                logger.LogError("Resource not found ({resource})", context.Request.Url);
                context.Response.SetStatus(HttpResponseCodes.NotFound);
            }
        }

        public Task Post(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(HttpResponseCodes.Forbidden);

            return Task.CompletedTask;
        }
    }
}
