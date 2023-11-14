using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppConnectionContext
    {
        public MiniAppConnectionContext(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        public IServiceProvider Services { get; }
        public ILoggerFactory LoggerFactory { get; }
    }
}
