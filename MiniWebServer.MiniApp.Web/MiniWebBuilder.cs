using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        public IServiceCollection Services { get; }

        public MiniWebBuilder(IServiceCollection? services = null) {
            if (services == null)
            {
                Services = new ServiceCollection();
            }
            else
            {
                Services = services;
            }
        }

        public IMiniWebBuilder UseRootDirectory(string path)
        {
            rootPath = path ?? throw new ArgumentNullException(path);

            return this;
        }
        public virtual MiniWeb Build()
        {
            var app = new MiniWeb();
            var sp = Services.BuildServiceProvider();

            if (useStaticFiles)
            {
                // it is good if we can prevent Service Locator pattern here and in action methods

                app.AddCallableService(
                    new StaticFileCallableService(new DirectoryInfo(rootPath), StaticMimeMapping.Instance, sp.GetService<ILogger<StaticFileCallableService>>()));
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
