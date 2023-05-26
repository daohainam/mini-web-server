using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Content;
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
        public MiniWebClientConnection(
            MiniWebConnectionConfiguration config,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken
            )
        {
            ConnectionId = config.Id;

            this.config = config;
            this.cancellationToken = cancellationToken;
            this.serviceProvider = serviceProvider;

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger<MiniWebClientConnection>();
        }

        public ulong ConnectionId { get; }

        private readonly MiniWebConnectionConfiguration config;
        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;
        private readonly IServiceProvider serviceProvider;

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
                    cancellationTokenSource.CancelAfter(config.ReadRequestTimeout);

                    logger.LogDebug("[{cid}] - Reading request...", ConnectionId);

                    var requestBuilder = new HttpWebRequestBuilder();
                    var responseBuilder = new HttpWebResponseBuilder();

                    if (!await ReadRequestAsync(requestPipeReader, requestBuilder, cancellationToken))
                    {
                        isKeepAlive = false; // we always close wrongly working connections

                        responseBuilder.SetStatusCode(HttpResponseCodes.BadRequest);
                        var response = responseBuilder.Build();

                        cancellationTokenSource.CancelAfter(config.ReadRequestTimeout);
                        logger.LogDebug("[{cid}] - Sending back response...", ConnectionId); // send back Bad Request
                        await SendResponseAsync(responsePipeWriter, response, cancellationToken);

                        break;
                    }
                    else
                    {
                        isKeepAlive = false;

                        var request = requestBuilder.Build();

                        var app = FindApp(request); // should we reuse apps???

                        if (app != null)
                        {
                            cancellationTokenSource.CancelAfter(config.ExecuteTimeout);
                            logger.LogDebug("[{cid}] - Processing request...", ConnectionId);
                            await ExecuteCallableAsync(request, responseBuilder, app, cancellationToken);
                        }

                        var response = responseBuilder.Build();

                        cancellationTokenSource.CancelAfter(config.ReadRequestTimeout);
                        logger.LogDebug("[{cid}] - Sending back response...", ConnectionId);
                        await SendResponseAsync(responsePipeWriter, response, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{cid}] - Error processing request", ConnectionId);
            }
            finally {
                CloseConnection();
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
                logger.LogError(ex, "[{cid}] - Error reading request", ConnectionId);
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

        private async Task ExecuteCallableAsync(HttpRequest request, HttpWebResponseBuilder responseBuilder, IMiniApp app, CancellationToken cancellationToken)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            try
            {
                await CallByMethod(app, request, responseBuilder, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{cid}] - Error calling resource", ConnectionId);
            }
        }

        private void CloseConnection()
        {
            logger.LogDebug("[{cid}] - Closing connection...", ConnectionId);
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

                    try
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
                    } catch (Exception ex)
                    {
                        logger.LogError(ex, "[{cid}] - Error executing action handler", ConnectionId);
                        response.SetStatus(HttpResponseCodes.InternalServerError);
                    }
                }
                else
                {
                    StandardResponseBuilderHelpers.NotFound(responseBuilder);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{cid}] - Error executing resource", ConnectionId);
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

            response.SetStatus(HttpResponseCodes.OK);
            response.SetContent(EmptyContent.Instance);
            response.AddHeader("Content-Type", "text/html");
            response.AddHeader("Connection", "close"); // todo: we will improve connection handling later
            

            return response;
        }

    }
}
