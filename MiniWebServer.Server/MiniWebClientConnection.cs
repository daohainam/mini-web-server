using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.Http.Helpers;
using MiniWebServer.Server.MiniApp;
using MiniWebServer.Server.Session;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
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

                MiniAppConnectionContext connectionContext = BuildMiniAppConnectionContext();

                while (isKeepAlive)
                {
                    cancellationTokenSource.CancelAfter(config.ReadRequestTimeout);

                    logger.LogDebug("[{cid}] - Reading request...", ConnectionId);

                    var requestBuilder = new HttpWebRequestBuilder();
                    // if time out we can simply close the connection
                    try
                    {
                        var readRequestResult = await config.ProtocolHandler.ReadRequestAsync(requestPipeReader, requestBuilder, cancellationToken);
                        if (!readRequestResult)
                        {
                            isKeepAlive = false; // we always close wrongly working connections

                            var response = new HttpResponse(HttpResponseCodes.BadRequest, config.ClientStream);

                            cancellationTokenSource.CancelAfter(config.SendResponseTimeout);
                            logger.LogDebug("[{cid}] - Sending back response...", ConnectionId); // send back Bad Request
                            await SendResponseAsync(response, cancellationToken);

                            break;
                        }
                        else
                        {
                            isKeepAlive = false; // we will close the connection if there is any error while building request
                            var requestId = config.RequestIdManager.GetNext();

                            var localEndPoint = config.TcpClient.Client.LocalEndPoint as IPEndPoint ?? throw new InvalidOperationException("TcpClient.Client.LocalEndPoint cannot be casted to IPEndPoint");

                            requestBuilder
                                .SetRequestId(requestId)
                                .SetHttps(config.IsHttps)
                                .SetPort(localEndPoint.Port);
                                var request = requestBuilder.Build();

                            isKeepAlive = request.KeepAliveRequested; // todo: we should have a look at how we manage a keep-alive connection later

                            var app = FindApp(request); // should we reuse apps???

                            var response = new HttpResponse(HttpResponseCodes.NotFound, config.ClientStream);
                            
                            if (app != null)
                            {
                                cancellationTokenSource.CancelAfter(config.ExecuteTimeout);
                                logger.LogDebug("[{cid}][{rid}] - Processing request...", ConnectionId, requestId);

                                // now we continue reading body part
                                CancellationTokenSource readBodyCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                                Task readBodyTask = config.ProtocolHandler.ReadBodyAsync(requestPipeReader, request, readBodyCancellationTokenSource.Token);
                                Task callMethodTask = CallByMethod(connectionContext, app, request, response, cancellationToken);

                                readBodyCancellationTokenSource.Cancel();

                                // todo: here we need to find a properly way to stop reading body after calling to middlewares and endpoints finished
                                Task.WaitAll(new Task[] { readBodyTask, callMethodTask }, cancellationToken);
                                logger.LogDebug("[{cid}][{rid}] - Done processing request...", ConnectionId, requestId);
                            }

                            var connectionHeader = response.Headers.Connection;
                            if (!"keep-alive".Equals(connectionHeader) && !"close".Equals(connectionHeader))
                            {
                                response.Headers.Connection = isKeepAlive ? "keep-alive" : "close";
                            }

                            cancellationTokenSource.CancelAfter(config.SendResponseTimeout);
                            logger.LogDebug("[{cid}][{rid}] - Sending back response...", ConnectionId, requestId);
                            await SendResponseAsync(response, cancellationToken);
                        }
                    } catch (OperationCanceledException)
                    {
                        isKeepAlive = false; 
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

        private MiniAppConnectionContext BuildMiniAppConnectionContext()
        {
            //var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var connectionContext = new MiniAppConnectionContext(serviceProvider.CreateScope().ServiceProvider);

            return connectionContext;
        }

        private async Task SendResponseAsync(HttpResponse response, CancellationToken cancellationToken)
        {
            await config.ProtocolHandler.WriteResponseAsync(response, cancellationToken);

            await response.Body.FlushAsync(cancellationToken);
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

        private void CloseConnection()
        {
            try
            {
                logger.LogDebug("[{cid}] - Closing connection...", ConnectionId);
                if (config.TcpClient.Connected)
                {
                    config.TcpClient.GetStream().Flush();
                }
                config.TcpClient.Close();
            }
            catch (Exception ex) { 
                logger.LogError(ex, "Failed to close connection");
            }   
        }

        private async Task CallByMethod(MiniAppConnectionContext connectionContext, IMiniApp app, HttpRequest request, IHttpResponse response, CancellationToken cancellationToken)
        {
            try
            {
                var context = BuildMiniContext(connectionContext, app, request, response);

                var action = app.Find(context);

                if (action != null)
                {
                    try
                    {
                        await action.InvokeAsync(context, cancellationToken);
                    } catch (Exception ex)
                    {
                        logger.LogError(ex, "[{cid}] - Error executing action handler", ConnectionId);
                        response.StatusCode = HttpResponseCodes.InternalServerError;
                    }
                }
                else
                {
                    StandardResponseBuilderHelpers.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{cid}] - Error executing resource", ConnectionId);
            }
        }

        private static MiniAppContext BuildMiniContext(MiniAppConnectionContext connectionContext, IMiniApp app, IHttpRequest request, IHttpResponse response)
        {
            ISession session = DefaultSession.Instance; // we don't have to alloc/dealloc memory parts which we never change

            // user will be set by Authentication middleware, we don't do anything here
            return new MiniAppContext(connectionContext, app, request, response, session, null);
        }
    }
}
