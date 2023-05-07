using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Configuration;
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
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("mini-web-server.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // Get values from the config given their key and their target type.
            ServerOptions serverOptions = config.Get<ServerOptions>() ?? new ServerOptions();

            var server = serverBuilder
                .UseOptions(serverOptions)
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
            services.AddLogging(loggingBuilder => loggingBuilder.AddLog4Net("log4net.xml"));
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