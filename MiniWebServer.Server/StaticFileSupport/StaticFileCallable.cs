using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Http.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.StaticFileSupport
{
    public class StaticFileCallable: ICallableResource
    {
        private readonly FileInfo file;
        private readonly ILogger logger;

        public StaticFileCallable(FileInfo file, ILogger logger)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static bool IsText(string mimeType) => mimeType.StartsWith("text/");

        public async Task OnGet(HttpRequest request, IHttpResponseBuilder responseObjectBuilder, IMimeTypeMapping mimeTypeMapping)
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
                        var content = await reader.ReadToEndAsync();

                        responseObjectBuilder.AddHeader("Content-Type", mimeType);
                        responseObjectBuilder.AddHeader("Content-Length", content.Length.ToString());
                        responseObjectBuilder.SetContent(new Http.Content.StringContent(content));

                        reader.Close(); // for a clear code, we call Close function even we are using 'using'
                    }
                    else
                    {
                        using var stream = file.OpenRead();
                        var length = file.Length;
                        var content = new byte[length];

                        stream.Read(content, 0, content.Length);

                        responseObjectBuilder.AddHeader("Content-Type", mimeType);
                        responseObjectBuilder.AddHeader("Content-Length", content.Length.ToString());
                        responseObjectBuilder.SetContent(new Http.Content.ByteArrayContent(content));

                        stream.Close(); 
                    }

                    StandardResponseBuilderHelpers.OK(responseObjectBuilder);
                } catch (Exception ex)
                {
                    logger.LogError(ex, message: null);
                    StandardResponseBuilderHelpers.InternalServerError(responseObjectBuilder);
                }
            }
            else
            {
                logger.LogError("Resource not found ({resource})", request.Url);
                StandardResponseBuilderHelpers.NotFound(responseObjectBuilder);
            }
        }
    }
}
