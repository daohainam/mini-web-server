using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeMapping;
using MiniWebServer.Abstractions;
using MiniWebServer.Configuration;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Quote;
using MiniWebServer.Server;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using MiniWebServer.Session;
using MiniWebServer.StaticFiles;
using System;
using System.Net.Http.Headers;
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
            var demoAppConfig = new ConfigurationBuilder().AddJsonFile("demoapp.json").Build().Get<DemoAppConfig>();

            IMiniApp app = BuildApp(serverBuilder.Services, demoAppConfig); // or server.CreateAppBuilder(); ?
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

        private static IMiniApp BuildApp(IServiceCollection services, DemoAppConfig? demoAppConfig)
        {
            var appBuilder = new MiniAppBuilder(services);

            appBuilder.UseAuthentication(new Authentication.AuthenticationOptions() { })
                .UseCookieAuthentication();

            if (demoAppConfig != null && demoAppConfig.Jwt != null
                && !string.IsNullOrEmpty(demoAppConfig.Jwt.Issuer)
                && !string.IsNullOrEmpty(demoAppConfig.Jwt.Audience)
                && !string.IsNullOrEmpty(demoAppConfig.Jwt.SecretKey)
                )
            {
                appBuilder.UseJwtAuthentication(new Authentication.JwtAuthenticationOptions(new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        AlgorithmValidator = (string algorithm, SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters) => { return "HS256".Equals(algorithm); },
                        ValidIssuer = demoAppConfig.Jwt.Issuer,
                        ValidAudience = demoAppConfig.Jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(demoAppConfig.Jwt.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    }));
            }
            appBuilder.UseSession();

            // endpoints should be called last 
            appBuilder.UseStaticFiles("wwwroot", defaultMaxAge: 7 * 24 * 3600); // defaultMaxAge = 7 days

            return appBuilder.Build();
        }

        private static IMiniApp MapRoutes(IMiniApp app)
        {
            app.MapGet("/helpcheck", async (context, cancellationToken) => {
                try
                {
                    await context.Session.LoadAsync();
                    var firstTime = context.Session.GetString("FirstTime");

                    if (firstTime == null)
                    {
                        context.Session.SetString("FirstTime", DateTime.Now.ToString());
                        await context.Session.SaveAsync();
                    }

                    context.Response.Content = new MiniApp.Content.StringContent($"Service status: OK {DateTime.Now}, First time: {firstTime}");
                } catch (Exception ex)
                {
                    context.Response.Content = new MiniApp.Content.StringContent($"Service status: ERROR {DateTime.Now} ({ex.Message})");
                }
            });

            app.MapGet("/file/textfile.txt", (context, cancellationToken) => {
                context.Response.Content = new MiniApp.Content.FileContent(Path.Combine("wwwroot", "textfile.txt"));

                return Task.CompletedTask;
            });

            app.MapGet("/quote/random", async (context, cancellationToken) => {
                string quote = await QuoteServiceFactory.GetQuoteService().GetRandomAsync();

                context.Response.Content = new MiniApp.Content.StringContent(quote);
            });

            app.MapGet("/string-api/toupper", (context, cancellationToken) => {
                string p = context.Request.QueryParameters["text"].Value ?? string.Empty;

                context.Response.Content = new MiniApp.Content.StringContent(p + " ===> " + p.ToUpper());

                return Task.CompletedTask;
            });

            app.MapPost("/post/form1", async (context, cancellationToken) => {
                var form = await context.Request.ReadFormAsync(context.Services.GetService<ILoggerFactory>(), cancellationToken);

                StringBuilder sb = new();
                foreach (var item in form)
                {
                    sb.Append("<div>");
                    sb.Append(item.Key);
                    sb.Append(": ");
                    sb.Append(item.Value);
                    sb.Append("</div>");
                }

                context.Response.Content = new MiniApp.Content.StringContent(sb.ToString());
            });

            app.MapPost("/post/form2", async (context, cancellationToken) => {
                var text = await context.Request.ReadAsStringAsync(cancellationToken);
                context.Response.Content = new MiniApp.Content.StringContent("Received as text: " + text);
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