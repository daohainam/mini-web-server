using Microsoft.Extensions.Logging;
using MimeMapping;
using MiniWebServer.MiniApp.Web.StaticFileSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web
{
    public class MiniWebBuilder: IMiniWebBuilder
    {
        private string rootPath = "wwwroot";
        private bool useStaticFiles = false;
        private readonly ILogger logger;

        public MiniWebBuilder(ILogger<MiniWebBuilder> logger) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IMiniWebBuilder UseRootDirectory(string path)
        {
            rootPath = path ?? throw new ArgumentNullException(path);

            return this;
        }
        public virtual MiniWeb Build()
        {
            var app = new MiniWeb();

            if (useStaticFiles)
            {
                app.AddCallableService(
                    new StaticFileCallableService(new DirectoryInfo(rootPath), StaticMimeMapping.Instance, logger));
            }

            return app;
        }

        public IMiniWebBuilder UseStaticFiles()
        {
            useStaticFiles = true; 
            
            return this;
        }
    }
}
