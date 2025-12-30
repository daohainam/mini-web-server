using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Abstractions;
using HttpMethod = global::MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.StaticFiles;

internal class StaticFilesMiddleware : IMiddleware
{
    private readonly ILogger<StaticFilesMiddleware> logger;
    private readonly DirectoryInfo directoryInfo;
    private readonly IMimeTypeMapping mimeTypeMapping;
    private readonly StaticFilesOptions options;

    public StaticFilesMiddleware(StaticFilesOptions options, IMimeTypeMapping? mimeTypeMapping, ILoggerFactory? loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(mimeTypeMapping);

        directoryInfo = new DirectoryInfo(options.Root);
        this.mimeTypeMapping = mimeTypeMapping;
        this.options = options;

        logger = loggerFactory != null ? loggerFactory.CreateLogger<StaticFilesMiddleware>() : NullLogger<StaticFilesMiddleware>.Instance;
    }
    public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        string url = context.Request.Url;

        if (!url.Contains("..") && directoryInfo.Exists)
        {
            if (url.StartsWith('/'))
            {
                url = url[1..];
            }
            url = url.Replace('/', Path.DirectorySeparatorChar);

            FileInfo? file = null;
            if (string.IsNullOrEmpty(url) && options.DefaultDocuments.Length != 0)
            {
                foreach (var document in options.DefaultDocuments)
                {
                    FileInfo f = new(Path.Combine(directoryInfo.FullName, document));
                    if (f.Exists)
                    {
                        file = f;
                        break;
                    }
                }
            }
            else
            {
                file = new(Path.Combine(directoryInfo.FullName, url));
                if (!file.Exists)
                {
                    var dir = new DirectoryInfo(file.FullName);
                    file = null;
                    if (options.DefaultDocuments.Length != 0 && dir.Exists)
                    {
                        foreach (var document in options.DefaultDocuments)
                        {
                            FileInfo f = new(Path.Combine(dir.FullName, document));
                            if (f.Exists)
                            {
                                file = f;
                                break;
                            }
                        }
                    }
                }
            }

            if (file != null)
            {
                if (context.Request.Method == HttpMethod.Get || context.Request.Method == HttpMethod.Head)
                {
                    try
                    {
                        string fileExt = file.Extension;
                        string mimeType = mimeTypeMapping.GetMimeMapping(fileExt);

                        var contentRange = context.Request.Headers.Range != null ? new MiniApp.Content.FileContentRange(
                            context.Request.Headers.Range.Parts[0].Start, context.Request.Headers.Range.Parts[0].End
                            ) : null;

                        context.Response.Headers.ContentType = mimeType;
                        if (context.Request.Method == HttpMethod.Head)
                        {
                            context.Response.Content = new MiniApp.Content.EmptyBodyFileContent(file);
                            contentRange = null;
                        }
                        else
                        {
                            long contentLength = file.Length;
                            if (contentRange != null)
                            {
                                if (contentRange.LastBytePosInclusive == null)
                                {
                                    contentRange.LastBytePosInclusive = file.Length - 1;
                                }
                                else if (contentRange.LastBytePosInclusive.Value > (file.Length + contentRange.FirstBytePosInclusive + 1))
                                {
                                    contentRange.LastBytePosInclusive = file.Length + contentRange.FirstBytePosInclusive + 1;
                                }
                            }

                            // I'm not sure if we can use compression together with Range requests, so I don't use for now
                            if (contentRange == null && options.UseCompression && context.Request.Headers.AcceptEncoding != null && context.Request.Headers.AcceptEncoding.Contains("br"))
                            {
                                if (options.MinimumFileSizeToCompress <= file.Length
                                    && (options.FileCompressionMimeTypes.Contains(mimeType))
                                    )
                                {
                                    context.Response.Content = new MiniApp.Content.CompressedFileContent(file, context, options.CompressionQuality, contentRange);
                                }
                                else
                                {
                                    context.Response.Content = new MiniApp.Content.FileContent(file, contentRange);
                                }
                            }
                            else
                            {
                                context.Response.Content = new MiniApp.Content.FileContent(file, contentRange);
                            }

                            if (contentRange != null)
                            {
                                context.Response.Headers.Add("Content-Range", $"bytes {contentRange.FirstBytePosInclusive}-{contentRange.LastBytePosInclusive}/{file.Length}");
                            }
                        }

                        //if (IsText(mimeType))
                        //{
                        //    using var reader = file.OpenText();
                        //    var content = await reader.ReadToEndAsync(cancellationToken);

                        //    context.Response.AddHeader("Content-Type", mimeType);
                        //    context.Response.AddHeader("Content-Length", content.Length.ToString());
                        //    context.Response.SetContent(new MiniApp.Content.StringContent(content));
                        //}
                        //else
                        //{
                        //    using var stream = file.OpenRead();
                        //    var length = file.Length;
                        //    var content = new byte[length];

                        //    stream.Read(content, 0, content.Length);

                        //    context.Response.AddHeader("Content-Type", mimeType);
                        //    context.Response.AddHeader("Content-Length", content.Length.ToString());
                        //    context.Response.SetContent(new MiniApp.Content.ByteArrayContent(content));
                        //}

                        if (options.CacheOptions.MaxAge > 0)
                        {
                            long maxAge = options.CacheOptions.MaxAge;

                            if (maxAge > 0)
                            {
                                context.Response.Headers.Add("Cache-Control", $"max-age={maxAge}");
                            }
                            else
                            {
                                context.Response.Headers.Add("Cache-Control", "no-cache"); // no-cache doesn't mean 'don't cache'
                            }
                        }

                        context.Response.StatusCode = contentRange != null ? HttpResponseCodes.PartialContent : HttpResponseCodes.OK;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing request");
                        context.Response.StatusCode = HttpResponseCodes.InternalServerError;
                    }
                }
                else
                {
                    context.Response.StatusCode = HttpResponseCodes.MethodNotAllowed;
                }
            }
            else
            {
                // instead of returning a 404 Not Found, we pass the request to next middleware
                await next.InvokeAsync(context, cancellationToken);
            }
        }
        else
        {
            // instead of returning a 404 Not Found, we pass the request to next middleware
            await next.InvokeAsync(context, cancellationToken);
        }
    }
}
