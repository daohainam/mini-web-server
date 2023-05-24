using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.Http.Helpers;
using MiniWebServer.Server.MiniApp;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server
{
    public class MiniWebClientConnection
    {
        public enum States
        {
            Pending,
            BuildingRequestObject,
            RequestObjectReady,
            CallingResource,
            CallingResourceReady,
            ResponseObjectReady,
            ReadyToClose
        }

        public MiniWebClientConnection(
            MiniWebConnectionConfiguration config,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken
            )
        {
            this.config = config;
            this.cancellationToken = cancellationToken;
            this.serviceProvider = serviceProvider;

            requestBuilder = new HttpWebRequestBuilder();
            responseBuilder = new HttpWebResponseBuilder();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger<MiniWebClientConnection>();
        }

        private readonly MiniWebConnectionConfiguration config;
        private readonly ILogger logger;
        public TimeSpan ExecuteTimeout { get; }

        private readonly CancellationToken cancellationToken;
        private readonly IServiceProvider serviceProvider;
        private readonly IHttpRequestBuilder requestBuilder;
        private readonly IHttpResponseBuilder responseBuilder;

        public async Task HandleRequestAsync()
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken); // we will use this to keep control on connection timeout
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            bool isKeepAlive = true;

            try
            {
                PipeReader requestPipeReader = PipeReader.Create(config.ClientStream);
                PipeWriter responsePipeWriter = PipeWriter.Create(config.ClientStream);

                while (isKeepAlive)
                {
                    if (!await ReadRequestAsync(requestPipeReader, requestBuilder, cancellationToken))
                    {
                        isKeepAlive = false; // we always close wrongly working connections
                        break;
                    }
                    else
                    {
                        isKeepAlive = false;

                        var request = requestBuilder.Build();

                        var app = FindApp(request); // should we reuse apps???

                        if (app != null)
                        {
                            await ExecuteCallableAsync(request, app, cancellationToken);
                        }

                        var response = responseBuilder.Build();
                        await SendResponseAsync(responsePipeWriter, response, cancellationToken);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request");
            }
        }

        private async Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            bool succeed = false;

            try
            {
                var readRequestResult = await config.ProtocolHandler.ReadRequestAsync(reader, requestBuilder, cancellationToken);
                if (readRequestResult)
                {
                    succeed = true;
                }
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "Error reading request");
            }

            return succeed;
        }

        private async Task SendResponseAsync(PipeWriter writer, HttpResponse response, CancellationToken cancellationToken)
        {
            await config.ProtocolHandler.WriteResponseAsync(writer, response, cancellationToken);

            await writer.FlushAsync(cancellationToken);
        }

        private IMiniApp? FindApp(HttpRequest request)
        {
            string host = request.Headers.Host;
            if (string.IsNullOrEmpty(host))
            {
                return null;
            }

            if (config.HostContainers.TryGetValue(host, out var container))
            {
                return container.App;
            }
            else
            {
                if (config.HostContainers.TryGetValue(string.Empty, out container)) // Host "" is a catch-all host
                {
                    return container.App;
                }
            }

            return null;
        }

        private async Task ExecuteCallableAsync(HttpRequest request, IMiniApp app, CancellationToken cancellationToken)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            try
            {
                await CallByMethod(app, request, responseBuilder, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling resource");
            }
        }

        private void CloseConnection()
        {
            config.TcpClient.GetStream().Flush();
            config.TcpClient.Close();
        }

        private async Task CallByMethod(IMiniApp app, HttpRequest httpRequest, IHttpResponseBuilder responseBuilder, CancellationToken cancellationToken)
        {
            try
            {
                var context = BuildMiniContext(app);
                var request = BuildMiniAppRequest(context, httpRequest);
                var response = BuildMiniAppResponse(context, responseBuilder);

                var action = app.Find(request);

                if (action != null)
                {
                    if (httpRequest.Method == HttpMethod.Get)
                    {
                        await action.Get(request, response, cancellationToken);
                    }
                    else if (httpRequest.Method == HttpMethod.Post)
                    {
                        await action.Post(request, response, cancellationToken);
                    }
                    else
                    {
                        StandardResponseBuilderHelpers.NotFound(responseBuilder);
                    }
                }
                else
                {
                    StandardResponseBuilderHelpers.NotFound(responseBuilder);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing resource");
            }
        }

        private static MiniAppContext BuildMiniContext(IMiniApp app)
        {
            return new MiniAppContext(app);
        }

        private static MiniRequest BuildMiniAppRequest(MiniAppContext context, HttpRequest httpRequest)
        {
            var request = new MiniRequest(context, httpRequest);

            return request;
        }

        private static MiniResponse BuildMiniAppResponse(MiniAppContext context, IHttpResponseBuilder responseBuilder)
        {
            var response = new MiniResponse(context, responseBuilder);

            return response;
        }

    }
}
