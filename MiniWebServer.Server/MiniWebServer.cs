using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniWebServer.Server.Abstractions;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class MiniWebServer : BackgroundService, IServer
    {
        private readonly MiniWebServerConfiguration config;
        private readonly ServiceProvider serviceProvider;
        private readonly IProtocolHandlerFactory protocolHandlerFactory;
        private readonly IDictionary<string, Host.Host> hostContainers;
        private readonly ILogger<MiniWebServer> logger;
        private TcpListener? server;
        private bool running;
        private ulong nextClientId = 0;

        private readonly ConcurrentDictionary<ulong, Task> clientTasks = new();

        private readonly CancellationTokenSource cancellationTokenSource; // we use this to cancel system-wise threads
        private readonly CancellationToken cancellationToken;

        public MiniWebServer(
            MiniWebServerConfiguration config,
            ServiceProvider serviceProvider,
            IProtocolHandlerFactory? protocolHandlerFactory,
            IDictionary<string, Host.Host>? hostContainers
            )
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.serviceProvider = serviceProvider;

            this.protocolHandlerFactory = protocolHandlerFactory ?? throw new ArgumentNullException(nameof(protocolHandlerFactory));
            this.hostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger<MiniWebServer>();

            running = false;
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;
        }

        private async Task HandleNewClientConnectionAsync(ulong connectionId, TcpClient tcpClient)
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
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        RemoteCertificateValidationCallback = ValidateClientCertificate
                    };

                    sslStream.AuthenticateAsServer(options);

                    stream = sslStream;
                }

                var client = new MiniWebClientConnection(
                    new MiniWebConnectionConfiguration(
                        connectionId,
                        tcpClient,
                        stream,
                        protocolHandlerFactory.Create(
                        new ProtocolHandlerConfiguration(ProtocolHandlerFactory.HTTP11, config.MaxRequestBodySize)
                    ), // A connection always starts with HTTP 1.1 
                    hostContainers,
                    TimeSpan.FromMilliseconds(config.ReadRequestTimeout),
                    TimeSpan.FromMilliseconds(config.SendResponseTimeout),
                    TimeSpan.FromMilliseconds(config.ConnectionTimeout),
                    config.ReadBufferSize
                    ),
                    serviceProvider,
                    cancellationToken
                );

                logger.LogInformation("[{cid}] - New connection added {tcpClient}", connectionId, tcpClient.Client.RemoteEndPoint);
                await client.HandleRequestAsync();
            }
            catch (AuthenticationException ex)
            {
                logger.LogError(ex, "Error accepting client");
                CloseConnection(tcpClient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accepting client");
                CloseConnection(tcpClient);
            }

            clientTasks.TryRemove(connectionId, out _);
        }

        private bool ValidateClientCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            logger.LogInformation("Validating certificate: {certificate}", certificate);

            return true; // accept all :D
        }

        private async Task ClientConnectionListeningProc(CancellationToken cancellationToken)
        {
            server = new(config.HttpEndPoint);
            server.Start();

            logger.LogInformation("Server started on {binding}", config.HttpEndPoint);

            while (running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync(cancellationToken);

                    var connectionId = Interlocked.Increment(ref nextClientId); // this function can be called concurrently (or not?) so we cannot use ++

                    Task t = HandleNewClientConnectionAsync(connectionId, client);

                    if (!clientTasks.TryAdd(connectionId, t))
                    {
                        // hope this will never happen
                        logger.LogError("Ooops! Cannot add task to client task list");
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Server socket stopped listening");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error accepting client socket");
                    break;
                }
            }

            server.Stop();

            Task.WaitAll(clientTasks.Values.ToArray(), TimeSpan.FromSeconds(30));
        }

        private static void CloseConnection(TcpClient tcpClient)
        {
            tcpClient.Close();
            tcpClient.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting web server...");

            if (config.Certificate != null)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            }

            running = true;

            await ClientConnectionListeningProc(stoppingToken);
        }

        public Task Start(CancellationToken? cancellationToken = null)
        {
            return ExecuteAsync(cancellationToken ?? CancellationToken.None);
        }

        public void Stop(CancellationToken? cancellationToken = null)
        {
            running = false;

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
        }
    }
}