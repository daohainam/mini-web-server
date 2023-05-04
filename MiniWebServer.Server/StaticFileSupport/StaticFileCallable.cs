using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Http.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task OnGet(HttpRequest request, IHttpResponseBuilder responseObjectBuilder)
        {
            if (file.Exists)
            {
                try
                {
                    using (var reader = file.OpenText())
                    {
                        var content = await reader.ReadToEndAsync();

                        responseObjectBuilder.AddHeader("Content-Type", "text/html");
                        responseObjectBuilder.AddHeader("Content-Length", content.Length.ToString());
                        responseObjectBuilder.SetContent(new Http.Content.StringContent(content));

                        StandardResponseBuilderHelpers.OK(responseObjectBuilder);
                    }
                } catch (Exception ex)
                {
                    logger.LogError(ex, message: null);
                }
            }
            else
            {
                StandardResponseBuilderHelpers.NotFound(responseObjectBuilder);
            }
        }
    }
}
