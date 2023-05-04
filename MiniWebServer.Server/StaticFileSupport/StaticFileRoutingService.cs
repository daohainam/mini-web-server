using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.StaticFileSupport
{
    public class StaticFileRoutingService : IRoutingService
    {
        private readonly DirectoryInfo directoryInfo;
        private readonly ILogger logger;

        public StaticFileRoutingService(DirectoryInfo directoryInfo, ILogger logger)
        {
            this.directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ICallableResource? FindRoute(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            if (directoryInfo.Exists)
            {
                if (url.StartsWith("/"))
                {
                    url = url[1..];
                }
                url = url.Replace('/', Path.DirectorySeparatorChar);

                var file = new FileInfo(Path.Combine(directoryInfo.FullName, url));
                if (file.Exists )
                {
                    return new StaticFileCallable(file, logger);
                }
            }
            return null;
        }
    }
}
