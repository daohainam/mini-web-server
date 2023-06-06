using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeMapping;
using MiniWebServer.Configuration;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Web;
using MiniWebServer.Quote;
using MiniWebServer.Server;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using System;
using System.Text;

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

            ConfigureServerServices(serverBuilder.Services);

            // Get values from the config given their key and their target type.
            ServerOptions serverOptions = config.Get<ServerOptions>() ?? new ServerOptions();
            serverBuilder = serverBuilder
                .UseOptions(serverOptions);

            IMiniApp app = BuildApp(); // or server.CreateAppBuilder(); ?
            app = MapRoutes(app);
            serverBuilder.AddHost(string.Empty, app);

            var server = serverBuilder.Build();
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    services => 
                    {
                        services.AddHostedService(services => server);
                        services.AddWindowsService(options =>
                        {
                            options.ServiceName = "Mini-Web-Server";
                        });
                    }
                )
                .ConfigureLogging((hostingContext, logging) =>
                {
                    //logging.AddLog4Net("log4net.xml").SetMinimumLevel(LogLevel.Debug);
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();
            host.Run();
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
            app.MapGet("/helpcheck", (context, cancellationToken) => {
                context.Response.SetContent(new MiniApp.Content.StringContent($"Service status: OK {DateTime.Now}"));

                return Task.CompletedTask;
            });

            app.MapGet("/file/textfile.txt", (context, cancellationToken) => {
                context.Response.SetContent(new MiniApp.Content.FileContent(Path.Combine("wwwroot", "textfile.txt")));

                return Task.CompletedTask;
            });

            app.MapGet("/quote/random", async (context, cancellationToken) => {
                string quote = await QuoteServiceFactory.GetQuoteService().GetRandomAsync();

                context.Response.SetContent(new MiniApp.Content.StringContent(quote));
            });

            app.MapGet("/string-api/toupper", (context, cancellationToken) => {
                string p = context.Request.QueryParameters["text"].Value ?? string.Empty;

                context.Response.SetContent(new MiniApp.Content.StringContent(p + " ===> " + p.ToUpper()));

                return Task.CompletedTask;
            });

            app.MapPost("/post/form1", async (context, cancellationToken) => {
                var form = await context.Request.ReadFormAsync(cancellationToken);

                StringBuilder sb = new();
                foreach (var item in form)
                {
                    sb.Append("<div>");
                    sb.Append(item.Key);
                    sb.Append(": ");
                    sb.Append(item.Value);
                    sb.Append("</div>");
                }

                context.Response.SetContent(new MiniApp.Content.StringContent(sb.ToString()));
            });

            app.MapPost("/post/form2", async (context, cancellationToken) => {
                var text = await context.Request.ReadAsStringAsync(cancellationToken);
                context.Response.SetContent(new MiniApp.Content.StringContent("Received as text: " + text));
            });

            return app;
        }

        private static void ConfigureServerServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddLog4Net("log4net.xml").SetMinimumLevel(LogLevel.Debug));
            services.AddDistributedMemoryCache();

            // todo: we should be able to safely remove the following registrations to use the defaults
            services.AddTransient<IHttpComponentParser, ByteSequenceHttpParser>(); 
            services.AddTransient<IProtocolHandlerFactory, ProtocolHandlerFactory>();
            services.AddSingleton<IMimeTypeMapping>(StaticMimeMapping.Instance);
        }
    }
}