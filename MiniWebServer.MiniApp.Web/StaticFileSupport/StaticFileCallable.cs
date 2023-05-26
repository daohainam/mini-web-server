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

        public async Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
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

                        response.AddHeader("Content-Type", mimeType);
                        response.AddHeader("Content-Length", content.Length.ToString());
                        response.SetContent(new Content.StringContent(content));
                    }
                    else
                    {
                        using var stream = file.OpenRead();
                        var length = file.Length;
                        var content = new byte[length];

                        stream.Read(content, 0, content.Length);

                        response.AddHeader("Content-Type", mimeType);
                        response.AddHeader("Content-Length", content.Length.ToString());
                        response.SetContent(new Content.ByteArrayContent(content));
                    }

                    response.SetStatus(HttpResponseCodes.OK);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, message: null);
                    response.SetStatus(HttpResponseCodes.InternalServerError);
                }
            }
            else
            {
                logger.LogError("Resource not found ({resource})", request.Url);
                response.SetStatus(HttpResponseCodes.NotFound);
            }
        }

        public Task Post(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            response.SetStatus(HttpResponseCodes.Forbidden);

            return Task.CompletedTask;
        }
    }
}
