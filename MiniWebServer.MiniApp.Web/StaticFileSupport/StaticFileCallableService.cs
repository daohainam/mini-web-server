using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web.StaticFileSupport
{
    public class StaticFileCallableService : ICallableService
    {
        private readonly DirectoryInfo directoryInfo;
        private readonly ILogger logger;
        private readonly IMimeTypeMapping mimeTypeMapping;

        public StaticFileCallableService(DirectoryInfo directoryInfo, IMimeTypeMapping mimeTypeMapping, ILogger? logger)
        {
            this.directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
            this.logger = logger ?? NullLogger.Instance;
            this.mimeTypeMapping = mimeTypeMapping ?? throw new ArgumentNullException(nameof(mimeTypeMapping));
        }

        public bool IsGetSupported => true;

        public ICallable? Find(IMiniAppRequest request)
        {
            string url = request.Url;

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
                    return new StaticFileCallable(file, mimeTypeMapping, logger);
                }
                else
                {
                    return NotFoundCallable.Instance;
                }
            }
            else
            {
                return NotFoundCallable.Instance;
            }

        }
    }
}
