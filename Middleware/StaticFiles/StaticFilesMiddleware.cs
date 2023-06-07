using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using HttpMethod = global::MiniWebServer.Abstractions.Http.HttpMethod;
using System.IO;
using MiniWebServer.Server.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace MiniWebServer.StaticFiles
{
    internal class StaticFilesMiddleware : IMiddleware
    {
        private readonly ILogger<StaticFilesMiddleware> logger;
        private readonly DirectoryInfo directoryInfo;
        private readonly IMimeTypeMapping mimeTypeMapping;

        public StaticFilesMiddleware(string rootDirectory, IMimeTypeMapping? mimeTypeMapping, ILoggerFactory? loggerFactory)
        {
            ArgumentException.ThrowIfNullOrEmpty(rootDirectory);
            ArgumentNullException.ThrowIfNull(mimeTypeMapping);

            directoryInfo = new DirectoryInfo(rootDirectory);
            this.mimeTypeMapping = mimeTypeMapping;
            logger = loggerFactory != null ? loggerFactory.CreateLogger<StaticFilesMiddleware>() : NullLogger<StaticFilesMiddleware>.Instance;
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            string url = context.Request.Url;

            if (directoryInfo.Exists)
            {
                if (url.StartsWith("/"))
                {
                    url = url[1..];
                }
                url = url.Replace('/', Path.DirectorySeparatorChar);

                var file = new FileInfo(Path.Combine(directoryInfo.FullName, url));
                if (file.Exists)
                {
                    if (context.Request.Method == HttpMethod.Get)
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
                                    context.Response.SetContent(new MiniApp.Content.StringContent(content));
                                }
                                else
                                {
                                    using var stream = file.OpenRead();
                                    var length = file.Length;
                                    var content = new byte[length];

                                    stream.Read(content, 0, content.Length);

                                    context.Response.AddHeader("Content-Type", mimeType);
                                    context.Response.AddHeader("Content-Length", content.Length.ToString());
                                    context.Response.SetContent(new MiniApp.Content.ByteArrayContent(content));
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
                    else
                    {
                        context.Response.SetStatus(HttpResponseCodes.MethodNotAllowed);
                    }
                }
                else
                {
                    await next.InvokeAsync(context, cancellationToken);
                }
            }
            else
            {
                await next.InvokeAsync(context, cancellationToken);
            }
        }

        private static bool IsText(string mimeType) => mimeType.StartsWith("text/");
    }
}