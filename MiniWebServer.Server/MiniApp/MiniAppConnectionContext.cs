using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Server.BodyReaders.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppConnectionContext
    {
        public MiniAppConnectionContext(IFormReaderFactory formReaderFactory, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            FormReaderFactory = formReaderFactory ?? throw new ArgumentNullException(nameof(formReaderFactory));
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IFormReaderFactory FormReaderFactory { get; }
        public IServiceProvider Services { get; }
        public ILoggerFactory LoggerFactory { get; }
    }
}
