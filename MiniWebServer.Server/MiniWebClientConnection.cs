using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Content;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.BodyReaders.Form;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.Http.Helpers;
using MiniWebServer.Server.MiniApp;
using MiniWebServer.Server.Session;
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

                MiniAppConnectionContext connectionContext = BuildMiniAppConnectionContext();

                while (isKeepAlive)
                {
                    cancellationTokenSource.CancelAfter(config.ReadRequestTimeout);

                    logger.LogDebug("[{cid}] - Reading request...", ConnectionId);

                    var requestBuilder = new HttpWebRequestBuilder();
                    var responseBuilder = new HttpWebResponseBuilder();

                    var readRequestResult = await config.ProtocolHandler.ReadRequestAsync(requestPipeReader, requestBuilder, cancellationToken);
                    if (!readRequestResult)
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
                        isKeepAlive = false; // we will close the connection if there is any error while building request
                        var request = requestBuilder.Build();

                        isKeepAlive = false; // request.KeepAliveRequested; // todo: we should have a look at how we manage a keep-alive connection later

                        var app = FindApp(request); // should we reuse apps???

                        if (app != null)
                        {
                            cancellationTokenSource.CancelAfter(config.ExecuteTimeout);
                            logger.LogDebug("[{cid}] - Processing request...", ConnectionId);

                            // now we continue reading body part
                            CancellationTokenSource readBodyCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            Task readBodyTask = config.ProtocolHandler.ReadBodyAsync(requestPipeReader, request, cancellationToken);

                            await CallByMethod(connectionContext, app, request, responseBuilder, cancellationToken);

                            readBodyCancellationTokenSource.Cancel();
                            readBodyTask.Wait(0, cancellationToken); // stop reading, remember that unprocessed bytes still remain in the socket buffer
                        }

                        var connectionHeader = responseBuilder.Headers.Connection;
                        if (!"keep-alive".Equals(connectionHeader) && !"close".Equals(connectionHeader))
                        {
                            responseBuilder.AddHeader("Connection", isKeepAlive ? "keep-alive" : "close");
                        }

                        var response = responseBuilder.Build();

                        cancellationTokenSource.CancelAfter(config.SendResponseTimeout);
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

        private MiniAppConnectionContext BuildMiniAppConnectionContext()
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var connectionContext = new MiniAppConnectionContext(new DefaultFormReaderFactory(), loggerFactory);

            return connectionContext;
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

        private void CloseConnection()
        {
            try
            {
                logger.LogDebug("[{cid}] - Closing connection...", ConnectionId);
                config.TcpClient.GetStream().Flush();
                config.TcpClient.Close();
            }
            catch (Exception ex) { 
                logger.LogError(ex, "Failed to close connection");
            }   
        }

        private async Task CallByMethod(MiniAppConnectionContext connectionContext, IMiniApp app, HttpRequest httpRequest, IHttpResponseBuilder responseBuilder, CancellationToken cancellationToken)
        {
            try
            {
                var request = BuildMiniAppRequest(connectionContext, httpRequest);
                var response = BuildMiniAppResponse(connectionContext, httpRequest, responseBuilder);
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

        private static MiniAppContext BuildMiniContext(MiniAppConnectionContext connectionContext, IMiniApp app, MiniRequest request, MiniResponse response)
        {
            ISession session = DefaultSession.Instance; // we don't have to alloc/dealloc memory parts which we never change

            return new MiniAppContext(connectionContext, app, request, response, session);
        }

        private static MiniRequest BuildMiniAppRequest(MiniAppConnectionContext connectionContext, HttpRequest httpRequest)
        {
            var request = new MiniRequest(connectionContext, httpRequest);

            return request;
        }

        private static MiniResponse BuildMiniAppResponse(MiniAppConnectionContext connectionContext, HttpRequest request, IHttpResponseBuilder responseBuilder)
        {
            var response = new MiniResponse(connectionContext, responseBuilder);

            response.AddCookies(request.Cookies);

            response.SetStatus(HttpResponseCodes.OK);
            response.SetContent(EmptyContent.Instance);
            response.AddHeader("Content-Type", "text/html");


            return response;
        }

    }
}
