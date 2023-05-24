using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeMapping;
using MiniWebServer.Configuration;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Web;
using MiniWebServer.Quote;
using MiniWebServer.Server;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.HttpParser.Http11;
using MiniWebServer.Server.MimeType;
using System;

namespace MiniWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IServerBuilder serverBuilder = new MiniWebServerBuilder();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("mini-web-server.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            ConfigureServices(serverBuilder.Services);

            // Get values from the config given their key and their target type.
            ServerOptions serverOptions = config.Get<ServerOptions>() ?? new ServerOptions();
            serverBuilder = serverBuilder
                .UseOptions(serverOptions)
                .UseThreadPoolSize(Environment.ProcessorCount);

            IMiniApp app = BuildApp(); // or server.CreateAppBuilder(); ?
            app = MapRoutes(app);
            serverBuilder.AddHost(string.Empty, app);

            var server = serverBuilder.Build();
            server.Start();

            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();

            server.Stop();
        }

        private static IMiniApp BuildApp()
        {
            var appBuilder = new MiniWebBuilder();

            appBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddLog4Net("log4net-demoapp.xml").SetMinimumLevel(LogLevel.Debug));

            appBuilder.UseRootDirectory("wwwroot")
            .UseStaticFiles();

            return appBuilder.Build();
        }

        private static IMiniApp MapRoutes(IMiniApp app)
        {
            app.MapGet("/helpcheck", (request, response, cancellationToken) => {
                response.SetContent(new MiniApp.Content.StringContent($"Service status: OK {DateTime.Now}"));

                return Task.CompletedTask;
            });

            app.MapGet("/quote/random", async (request, response, cancellationToken) => {
                string quote = await QuoteServiceFactory.GetQuoteService().GetRandomAsync();

                response.SetContent(new MiniApp.Content.StringContent(quote));
            });

            return app;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddLog4Net("log4net.xml").SetMinimumLevel(LogLevel.Debug));
            services.AddDistributedMemoryCache();
            services.AddTransient<IHttp11Parser, RegexHttp11Parsers>();
            services.AddTransient<IProtocolHandlerFactory, ProtocolHandlerFactory>();
            services.AddSingleton<IMimeTypeMapping>(StaticMimeMapping.Instance);
        }
    }
}