using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Server;

namespace MiniWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var serverBuilder = serviceProvider.GetService<IServerBuilder>();

            if (serverBuilder == null)
            {
                serviceProvider.GetService<ILogger>()?.LogError("Error creating ServerBuilder");
                return;
            }

            ;
            var server = serverBuilder
                .UseHttpPort(8080)
                .UseStaticFiles()
                //.UseCache(serviceProvider.GetService<IDistributedCache>())
                .Build();

            server.Start();

            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();

            server.Stop();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddDistributedMemoryCache();

            services.AddTransient<IServerBuilder>(
                services => new MiniWebServerBuilder(
                    services.GetService<ILogger<MiniWebServerBuilder>>(),
                    services.GetService<IDistributedCache>()
                    )
                );
        }
    }
}