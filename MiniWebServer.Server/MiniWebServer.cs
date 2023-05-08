using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Host;
using MiniWebServer.Server.Http.Helpers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class MiniWebServer : IServer
    {
        private readonly MiniWebServerConfiguration config;
        private readonly IProtocolHandlerFactory protocolHandlerFactory;
        private readonly IDictionary<string, HostContainer> hostContainers;
        private readonly IMimeTypeMapping mimeTypeMapping;
        private readonly ILogger logger;
        private TcpListener? server;
        private bool running;
        private int nextClientId = 1;
        private int threadInThreadPoolCount = 0;
        private readonly ConcurrentQueue<MiniWebClientConnection> waitingClients = new();
        private static EventWaitHandle waitingClientsWaitHandle = new(false, EventResetMode.AutoReset);
        private CancellationTokenSource acceptCancellationTokenSource = new();
        private CancellationToken acceptCancellationToken;
        private bool disposed = false;

        public MiniWebServer(
            MiniWebServerConfiguration config, 
            IProtocolHandlerFactory protocolHandlerFactory,
            IDictionary<string, HostContainer> hostContainers,
            IMimeTypeMapping mimeTypeMapping,
            ILogger logger
            )
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.protocolHandlerFactory = protocolHandlerFactory ?? throw new ArgumentNullException(nameof(protocolHandlerFactory));
            this.hostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            this.mimeTypeMapping = mimeTypeMapping ?? throw new ArgumentNullException(nameof(mimeTypeMapping));
            this.logger = logger;

            running = false;
        }

        public void Start()
        {
            logger.LogInformation("Starting web server...");

            if (config.Certificate != null)
            {
                // lower TLS versions are obsoleted, we only use 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            running = true;

            acceptCancellationToken = acceptCancellationTokenSource.Token;

            new Thread(ClientConnectionListeningProc) { IsBackground = false }.Start();

            for (int i = 1; i <= config.ThreadPoolSize; i++)
            {
                var thread = new Thread(ClientConnectionProcessingProc) { IsBackground = false };
                thread.Start(i);
            }

        }

        private async Task HandleNewClientConnection(TcpClient tcpClient)
        {
            Stream stream = tcpClient.GetStream();
            if (config.Certificate != null)
            {
                var sslStream = new SslStream(stream);

                SslServerAuthenticationOptions options = new()
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                    {
                        SslApplicationProtocol.Http11
                    },
                    ServerCertificate = config.Certificate,
                    EnabledSslProtocols = SslProtocols.None, // use the system default version
                    ClientCertificateRequired = false,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                };

                await sslStream.AuthenticateAsServerAsync(options);

                stream = sslStream;
            }

            var client = new MiniWebClientConnection(
                nextClientId++,
                tcpClient,
                stream,
                protocolHandlerFactory.Create(ProtocolHandlerFactory.HTTP11), // A connection always starts with HTTP 1.1 
                MiniWebClientConnection.States.Pending
            ); ;

            waitingClients.Enqueue(client);
            logger.LogInformation("New client connected");

            waitingClientsWaitHandle.Set();
        }

        public void Stop()
        {
            running = false;
            acceptCancellationTokenSource.Cancel();
            server?.Stop();

            // wait for all client threads stopped, we can use wait handles to make it more resource effective, but we use a loop here because it is more understandable :)
            int seconds10 = 15 * 10; // we will wait max 15 seconds
            while (threadInThreadPoolCount > 0 && seconds10 > 0)
            {
                Task.Delay(100).Wait();

                seconds10--;
            }
        }

        private async void ClientConnectionListeningProc()
        {
            server = new(config.HttpEndPoint);
            server.Start();

            logger.LogInformation("Server started on {binding}", config.HttpEndPoint);

            while (running)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync(acceptCancellationToken);

                    await HandleNewClientConnection(client);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Server socket stopped listening");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error accepting client socket");
                }
            }
        }

        private async void ClientConnectionProcessingProc(object? data)
        {
            int n = (int?)data ?? 0;
            logger.LogInformation("Starting ThreadPool.Thread #{n}", n);

            Interlocked.Increment(ref threadInThreadPoolCount); // why don't we simply use threadInThreadPoolCount++ here? :)

            while (running)
            {
                logger.LogDebug("ThreadPool.Thread {n} processing...", n);

                // wait for 1 second and process if there is at least one item in the queue
                if (waitingClientsWaitHandle.WaitOne(1000) && running)
                {
                    if (waitingClients.TryDequeue(out var client))
                    {
                        var newClient = await ProcessClientConnection(client);
                        if (newClient != null)
                        {
                            waitingClients.Enqueue(newClient);
                            waitingClientsWaitHandle.Set();
                        }
                    }
                }
            }

            logger.LogInformation("ThreadPool.Thread #{n} stopped", n);
            Interlocked.Decrement(ref threadInThreadPoolCount);
        }

        private async Task<MiniWebClientConnection?> ProcessClientConnection(MiniWebClientConnection client)
        {
            logger.LogDebug("Processing client connection: {client}", client.Id);

            try
            {
                if (client.State == MiniWebClientConnection.States.Pending || client.State == MiniWebClientConnection.States.BuildingRequestObject)
                {
                    var state = await client.ProtocolHandler.ReadRequest(client.ClientStream, client.RequestObjectBuilder, client.ProtocolHandlerData);
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
                    if (hostContainers.TryGetValue(requestObject.Headers.Host, out var hostContainer)
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
                    client.State = NextState(client.State);
                }
                else if (client.State == MiniWebClientConnection.States.ResponseObjectReady)
                {
                    await client.ProtocolHandler.SendResponse(client.ClientStream, client.ResponseObjectBuilder, client.ProtocolHandlerData);

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
                    await callable.OnGet(request, responseObjectBuilder, mimeTypeMapping);
                }
                else
                {
                    StandardResponseBuilderHelpers.MethodNotAllowed(responseObjectBuilder);
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