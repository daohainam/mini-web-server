using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Host;
using MiniWebServer.Server.Http.Helpers;
using MiniWebServer.Server.ProtocolHandlers;
using MiniWebServer.Server.Routing;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;

namespace MiniWebServer.Server
{
    public class MiniWebServer : IServer
    {
        private readonly MiniWebServerConfiguration config;
        private readonly IProtocolHandlerFactory protocolHandlerFactory;
        private readonly Dictionary<string, HostContainer> hostContainers;
        private readonly ILogger logger;
        private TcpListener? server;
        private bool running;
        private int nextClientId = 1;
        private readonly ConcurrentQueue<MiniWebClientConnection> waitingClients = new();
        private static EventWaitHandle clientConnectionWaitHandle = new(false, EventResetMode.AutoReset);
        private CancellationTokenSource acceptCancellationTokenSource = new();
        private CancellationToken acceptCancellationToken;
        private bool disposed = false;

        public MiniWebServer(
            MiniWebServerConfiguration config, 
            IProtocolHandlerFactory protocolHandlerFactory,
            Dictionary<string, HostContainer> hostContainers,
            ILogger logger
            )
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.protocolHandlerFactory = protocolHandlerFactory ?? throw new ArgumentNullException(nameof(protocolHandlerFactory));
            this.hostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            this.logger = logger;

            running = false;
        }

        public void Start()
        {
            logger.LogInformation("Starting web server...");

            running = true;

            acceptCancellationToken = acceptCancellationTokenSource.Token;

            new Thread(ClientConnectionListeningProc) { IsBackground = false }.Start();

            for (int i = 1; i <= config.ThreadPoolSize; i++)
            {
                var thread = new Thread(ClientConnectionProcessingProc) { IsBackground = false };
                thread.Start(i);
            }

        }

        private void HandleNewClientConnection(TcpClient tcpClient)
        {
            var client = new MiniWebClientConnection(
                nextClientId++,
                tcpClient,
                protocolHandlerFactory.Create(ProtocolHandlerFactory.HTTP11), // A connection always starts with HTTP 1.1 
                MiniWebClientConnection.States.Pending
            );

            waitingClients.Enqueue(client);
            logger.LogInformation("New client connected");

            clientConnectionWaitHandle.Set();
        }

        public void Stop()
        {
            running = false;
            acceptCancellationTokenSource.Cancel();
            server?.Stop();

            Task.Delay(5000).Wait();
        }

        private async void ClientConnectionListeningProc()
        {
            server = new(config.HttpEndPoint);
            server.Start();

            logger.LogInformation("Server has started on {binding}.", config.HttpEndPoint);

            while (running)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync(acceptCancellationToken);

                    HandleNewClientConnection(client);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, null);
                }
            }
        }

        private async void ClientConnectionProcessingProc(object? data)
        {
            int n = (int?)data ?? 0;
            logger.LogInformation("Starting ThreadPool.Thread {n}", n);

            while (running)
            {
                logger.LogDebug("ThreadPool.Thread {n} processing...", n);

                clientConnectionWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                if (running)
                {
                    if (waitingClients.TryDequeue(out var client))
                    {
                        var newClient = await ProcessClientConnection(client);
                        if (newClient != null)
                        {
                            waitingClients.Enqueue(newClient);
                            clientConnectionWaitHandle.Set();
                        }
                    }
                }
            }

            logger.LogInformation("ThreadPool.Thread {n} stopped", n);
        }

        private async Task<MiniWebClientConnection?> ProcessClientConnection(MiniWebClientConnection client)
        {
            logger.LogDebug("Processing client connection: {client}", client.Id);

            try
            {
                if (client.State == MiniWebClientConnection.States.Pending || client.State == MiniWebClientConnection.States.BuildingRequestObject)
                {
                    var state = await client.ProtocolHandler.ReadRequest(client.TcpClient, client.RequestObjectBuilder, client.ProtocolHandlerData);
                    if (state == ProtocolHandlerStates.BuildRequestStates.Failed)
                    {
                        StandardResponseBuilderHelpers.BadRequest(client.ResponseObjectBuilder);
                        client.State = MiniWebClientConnection.States.RequestObjectReady;
                    }
                    else if (state == ProtocolHandlerStates.BuildRequestStates.Succeeded)
                    {
                        client.State = MiniWebClientConnection.States.RequestObjectReady;
                    }
                }
                else if (client.State == MiniWebClientConnection.States.RequestObjectReady)
                {
                    var requestObject = client.RequestObjectBuilder.Build();
                    if (hostContainers.TryGetValue(requestObject.Host, out var hostContainer)
                        || hostContainers.TryGetValue(string.Empty, out hostContainer)) {
                        // find a callable resource using IRoutingService
                        var callable = hostContainer.RoutingService.FindRoute(requestObject.Url);

                        if (callable != null)
                        {
                            await CallByMethod(callable, requestObject.Method, requestObject, client.ResponseObjectBuilder);

                            client.State = NextState(client.State);
                        }
                        else
                        {
                            StandardResponseBuilderHelpers.NotFound(client.ResponseObjectBuilder);
                            client.State = MiniWebClientConnection.States.ResponseObjectReady;
                        }
                    }
                    else
                    {
                        // unknown host
                        StandardResponseBuilderHelpers.NotFound(client.ResponseObjectBuilder);
                        client.State = MiniWebClientConnection.States.ResponseObjectReady;
                    }
                }
                else if (client.State == MiniWebClientConnection.States.CallingResource)
                {
                    client.State = NextState(client.State);
                }
                else if (client.State == MiniWebClientConnection.States.CallingResourceReady)
                {
                    StandardResponseBuilderHelpers.OK(client.ResponseObjectBuilder);

                    client.State = NextState(client.State);
                }
                else if (client.State == MiniWebClientConnection.States.ResponseObjectReady)
                {
                    await client.ProtocolHandler.SendResponse(client.TcpClient, client.ResponseObjectBuilder, client.ProtocolHandlerData);

                    client.State = NextState(client.State);
                }
                else if (client.State == MiniWebClientConnection.States.ReadyToClose)
                {
                    ReleaseResources(client);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing client connection");

                client.TcpClient.Close();

                return null;
            }

            return client;
        }

        private async Task CallByMethod(ICallableResource callable, HttpMethod method, HttpRequest request, IHttpResponseBuilder responseObjectBuilder)
        {
            try
            {
                if (method == HttpMethod.Get)
                {
                    await callable.OnGet(request, responseObjectBuilder);
                }
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error executing resource");
            }
        }

        private static void ReleaseResources(MiniWebClientConnection client)
        {
            CloseConnection(client.TcpClient);
        }

        private static void CloseConnection(TcpClient tcpClient)
        {
            tcpClient.Close();
        }

        private static MiniWebClientConnection.States NextState(MiniWebClientConnection.States state) => state switch
        {
            MiniWebClientConnection.States.Pending => MiniWebClientConnection.States.BuildingRequestObject,
            MiniWebClientConnection.States.BuildingRequestObject => MiniWebClientConnection.States.RequestObjectReady,
            MiniWebClientConnection.States.RequestObjectReady => MiniWebClientConnection.States.CallingResource,
            MiniWebClientConnection.States.CallingResource => MiniWebClientConnection.States.CallingResourceReady,
            MiniWebClientConnection.States.CallingResourceReady => MiniWebClientConnection.States.ResponseObjectReady,
            MiniWebClientConnection.States.ResponseObjectReady => MiniWebClientConnection.States.ReadyToClose,
            _ => throw new InvalidOperationException()
        };

        #region IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                if (running)
                {
                    Stop();
                }

                disposed = true;
            }
        }
        #endregion
    }
}