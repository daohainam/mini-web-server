using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
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
        private readonly IDictionary<string, Host.Host> hostContainers;
        private readonly ILogger<MiniWebServer> logger;
        private TcpListener? server;
        private bool running;
        private int nextClientId = 1;
        private int threadInThreadPoolCount = 0;
        private readonly ConcurrentQueue<MiniWebClientConnection> waitingClients = new();
        private static readonly EventWaitHandle waitingClientsWaitHandle = new(false, EventResetMode.AutoReset);
        private bool disposed = false;

        private CancellationTokenSource cancellationTokenSource; // we use this to cancel system-wise threads
        private CancellationToken cancellationToken;

        public MiniWebServer(
            MiniWebServerConfiguration config, 
            IProtocolHandlerFactory? protocolHandlerFactory,
            IDictionary<string, Host.Host>? hostContainers,
            ILogger<MiniWebServer> logger
            )
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.protocolHandlerFactory = protocolHandlerFactory ?? throw new ArgumentNullException(nameof(protocolHandlerFactory));
            this.hostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            this.logger = logger;

            running = false;
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;
        }

        public Task Start()
        {
            logger.LogInformation("Starting web server...");

            if (config.Certificate != null)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            }

            running = true;

            return ClientConnectionListeningProc();

            // create threads to process client data, it is actually not efficient to do this way, but it can demonstrate how a thread pool works
            // normally we create it's own class to make the code 'clean', but we will soon change to .NET's ThreadPool https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool?view=net-7.0
            // so just let it be :)
            //for (int i = 1; i <= config.ThreadPoolSize; i++)
            //{
            //    var thread = new Thread(ClientConnectionProcessingProc) { IsBackground = true };
            //    thread.Start(i);
            //}

        }

        private async Task HandleNewClientConnectionAsync(TcpClient tcpClient)
        {
            try
            {
                Stream stream = tcpClient.GetStream();
                if (config.Certificate != null)
                {
                    var sslStream = new SslStream(stream);

                    SslServerAuthenticationOptions options = new()
                    {
                        ApplicationProtocols = new()
                        {
                            SslApplicationProtocol.Http11
                        },
                        ServerCertificate = config.Certificate,
                        EnabledSslProtocols = SslProtocols.None, // use the system default version
                        ClientCertificateRequired = false,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    };

                    sslStream.AuthenticateAsServer(options);

                    stream = sslStream;
                }

                var client = new MiniWebClientConnection(
                    nextClientId++,
                    tcpClient,
                    stream,
                    protocolHandlerFactory.Create(ProtocolHandlerFactory.HTTP11), // A connection always starts with HTTP 1.1 
                    hostContainers,
                    TimeSpan.FromMilliseconds(config.ConnectionTimeout),
                    logger,
                    cancellationToken
                );

                //waitingClients.Enqueue(client);
                logger.LogInformation("New client connected {tcpClient}", tcpClient.Client.RemoteEndPoint);
                await client.HandleRequestAsync();

                //waitingClientsWaitHandle.Set();
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error accepting client");
                CloseConnection(tcpClient);
            }
        }

        public void Stop()
        {
            running = false;
            server?.Stop();

            cancellationTokenSource.Cancel();

            // wait for all client threads stopped, we can use wait handles to make it more resource effective, but we use a loop here because it is more understandable :)
            int seconds10 = 15 * 10; // we will wait max 15 seconds
            for (int i = threadInThreadPoolCount; i >= 0; i-- )
            {
                waitingClientsWaitHandle.Set();
            }

            while (threadInThreadPoolCount > 0 && seconds10 > 0)
            {
                waitingClientsWaitHandle.Set();

                Task.Delay(100).Wait();

                seconds10--;
            }
        }

        private async Task ClientConnectionListeningProc()
        {
            server = new(config.HttpEndPoint);
            server.Start();

            logger.LogInformation("Server started on {binding}", config.HttpEndPoint);

            while (running)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync(cancellationToken);

                    await HandleNewClientConnectionAsync(client);
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

        /// <summary>
        /// Thread in threadpool, these threads will be runnable state (pe unless there are something in the task queue to process
        /// Using .NET's ThreadPool class and async/await are better in performance since they can control pool running threads
        /// </summary>
        /// <param name="data"></param>
        //private void ClientConnectionProcessingProc(object? data)
        //{
        //    int n = (int?)data ?? 0;
        //    logger.LogInformation("Starting ThreadPool.Thread #{n}", n);

        //    Interlocked.Increment(ref threadInThreadPoolCount); // why don't we simply use threadInThreadPoolCount++ here? :)

        //    while (running)
        //    {
        //        // wait for 1 second and process if there is at least one item in the queue
        //        if (waitingClientsWaitHandle.WaitOne() && running)
        //        {
        //            if (waitingClients.TryDequeue(out var client))
        //            {
        //                var newClient = ProcessClientConnection(client).GetAwaiter().GetResult();
        //                if (newClient != null)
        //                {
        //                    waitingClients.Enqueue(newClient);
        //                    waitingClientsWaitHandle.Set();
        //                }
        //            }
        //        }
        //    }

        //    logger.LogInformation("ThreadPool.Thread #{n} stopped", n);
        //    Interlocked.Decrement(ref threadInThreadPoolCount);
        //}

        //private async Task<MiniWebClientConnection?> ProcessClientConnection(MiniWebClientConnection client)
        //{
        //    logger.LogInformation("Processing client connection: {client}", client.Id);

        //    try
        //    {
        //        if (client.State == MiniWebClientConnection.States.Pending || client.State == MiniWebClientConnection.States.BuildingRequestObject)
        //        {
        //            // todo: we must have a mechanism to control request reading timeout
        //            var state = await client.ProtocolHandler.ReadRequestAsync(client.ClientStream, client.RequestObjectBuilder, client.ProtocolHandlerData);
        //            if (state == ProtocolHandlerStates.BuildRequestStates.Failed)
        //            {
        //                StandardResponseBuilderHelpers.BadRequest(client.ResponseObjectBuilder);
        //                client.KeepAlive = false;
        //                client.State = MiniWebClientConnection.States.RequestObjectReady;
        //            }
        //            else if (state == ProtocolHandlerStates.BuildRequestStates.Succeeded)
        //            {
        //                client.State = MiniWebClientConnection.States.RequestObjectReady;
        //            }
        //            else if (state == ProtocolHandlerStates.BuildRequestStates.InProgressWithNoData)
        //            {
        //                if (client.ConnectionTimeoutTime <= DateTime.Now) // if no data received after config.ConnectionTimeout ms, we close the connection
        //                {
        //                    logger.LogInformation("Connection {connection} timed-out", client.TcpClient.Client.RemoteEndPoint);
        //                    client.KeepAlive = false;
        //                    client.State = MiniWebClientConnection.States.ResponseObjectReady;
        //                }
        //            }
        //            else
        //            {
        //                client.ConnectionTimeoutTime = DateTime.Now.AddMilliseconds(config.ConnectionTimeout);
        //            }
        //        }
        //        else if (client.State == MiniWebClientConnection.States.RequestObjectReady)
        //        {
        //            var requestObject = client.RequestObjectBuilder.Build();

        //            client.KeepAlive = string.IsNullOrEmpty(requestObject.Headers.Connection) || string.Equals("keep-alive", requestObject.Headers.Connection, StringComparison.OrdinalIgnoreCase);

        //            if (hostContainers.TryGetValue(requestObject.Headers.Host, out var hostContainer)
        //                || hostContainers.TryGetValue(string.Empty, out hostContainer)) {
        //                // find a callable resource using IRoutingService
        //                var callable = hostContainer.RoutingService.FindRoute(requestObject.Url);

        //                if (callable != null)
        //                {
        //                    await CallByMethod(callable, requestObject.Method, requestObject, client.ResponseObjectBuilder);

        //                    client.State = NextState(client.State, client.KeepAlive);
        //                }
        //                else
        //                {
        //                    StandardResponseBuilderHelpers.NotFound(client.ResponseObjectBuilder);
        //                    client.State = MiniWebClientConnection.States.ResponseObjectReady;
        //                }
        //            }
        //            else
        //            {
        //                // unknown host
        //                StandardResponseBuilderHelpers.NotFound(client.ResponseObjectBuilder);
        //                client.State = MiniWebClientConnection.States.ResponseObjectReady;
        //            }
        //        }
        //        else if (client.State == MiniWebClientConnection.States.CallingResource)
        //        {
        //            client.State = NextState(client.State, client.KeepAlive);
        //        }
        //        else if (client.State == MiniWebClientConnection.States.CallingResourceReady)
        //        {
        //            client.State = NextState(client.State, client.KeepAlive);
        //        }
        //        else if (client.State == MiniWebClientConnection.States.ResponseObjectReady)
        //        {
        //            if (client.KeepAlive)
        //            {
        //                client.ResponseObjectBuilder.SetHeaderConnection("keep-alive");
        //            }
        //            else
        //            {
        //                client.ResponseObjectBuilder.SetHeaderConnection("close");
        //            }

        //            var response = client.ResponseObjectBuilder.Build();
        //            await client.ProtocolHandler.SendResponseAsync(client.ClientStream, response, client.ProtocolHandlerData);
        //            client.ProtocolHandler.Reset(client.ProtocolHandlerData);

        //            client.State = NextState(client.State, client.KeepAlive); // if keep-alive we start processing next message, else we release and close connection
        //        }
        //        else if (client.State == MiniWebClientConnection.States.ReadyToClose)
        //        {
        //            ReleaseResources(client);

        //            return null; // return null to not enqueue the task again
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error processing client connection");

        //        client.TcpClient.Close();

        //        return null;
        //    }

        //    return client;
        //}



        private static void ReleaseResources(MiniWebClientConnection client)
        {
            CloseConnection(client.TcpClient);
        }

        private static void CloseConnection(TcpClient tcpClient)
        {
            tcpClient.Close();
            tcpClient.Dispose();
        }

        private static MiniWebClientConnection.States NextState(MiniWebClientConnection.States state, bool keepAlive) => state switch
        {
            MiniWebClientConnection.States.Pending => MiniWebClientConnection.States.BuildingRequestObject,
            MiniWebClientConnection.States.BuildingRequestObject => MiniWebClientConnection.States.RequestObjectReady,
            MiniWebClientConnection.States.RequestObjectReady => MiniWebClientConnection.States.CallingResource,
            MiniWebClientConnection.States.CallingResource => MiniWebClientConnection.States.CallingResourceReady,
            MiniWebClientConnection.States.CallingResourceReady => MiniWebClientConnection.States.ResponseObjectReady,
            MiniWebClientConnection.States.ResponseObjectReady => keepAlive ? MiniWebClientConnection.States.Pending : MiniWebClientConnection.States.ReadyToClose,
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