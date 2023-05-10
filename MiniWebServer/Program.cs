using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeMapping;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.HttpParser.Http11;
using MiniWebServer.Configuration;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server;
using MiniWebServer.Server.MimeType;

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
                .UseDependencyInjectionService(serviceProvider)
                .UseOptions(serverOptions)
                .UseThreadPoolSize(Environment.ProcessorCount)
                .UseStaticFiles()
                .Build();

            server.Start();

            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();

            server.Stop();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddLog4Net("log4net.xml"));
            services.AddDistributedMemoryCache();
            services.AddTransient<IHttp11Parser, RegexHttp11Parsers>();
            services.AddTransient<IProtocolHandlerFactory, ProtocolHandlerFactory>();
            services.AddSingleton<IMimeTypeMapping>(StaticMimeMapping.GetInstance());
            services.AddTransient<IServerBuilder>(
                services => new MiniWebServerBuilder(
                    services.GetService<ILogger<MiniWebServerBuilder>>(),
                    services.GetService<IDistributedCache>()
                    )
                );
        }
    }
}