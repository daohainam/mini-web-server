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
        private bool running;
        private ulong nextClientId = 0;
        private readonly IRequestIdManager requestIdManager;

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

            requestIdManager = serviceProvider.GetService<IRequestIdManager>() ?? new RequestIdManager();
        }

        private async Task HandleNewClientConnectionAsync(ulong connectionId, MiniWebServerBindingConfiguration binding, TcpClient tcpClient)
        {
            try
            {
                Stream stream = tcpClient.GetStream();
                bool isHttps = false;
                if (binding.Certificate != null)
                {
                    var sslStream = new SslStream(stream);

                    SslServerAuthenticationOptions options = new()
                    {
                        ApplicationProtocols =
                        [
                            SslApplicationProtocol.Http11
                        ],
                        ServerCertificate = binding.Certificate,
                        EnabledSslProtocols = SslProtocols.None, // use the system default version
                        ClientCertificateRequired = false,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        RemoteCertificateValidationCallback = ValidateClientCertificate
                    };

                    await sslStream.AuthenticateAsServerAsync(options);

                    stream = sslStream;
                    isHttps = true;
                }

                using var serviceScope = serviceProvider.CreateScope();
                var client = new MiniWebClientConnection(
                    new MiniWebConnectionConfiguration(
                        connectionId,
                        tcpClient,
                        stream,
                        isHttps,
                        protocolHandlerFactory.Create(
                        new ProtocolHandlerConfiguration(ProtocolHandlerFactory.HTTP11, config.MaxRequestBodySize)
                    ), // A connection always starts with HTTP 1.1 
                    hostContainers,
                    requestIdManager,
                    TimeSpan.FromMilliseconds(config.ReadRequestTimeout),
                    TimeSpan.FromMilliseconds(config.SendResponseTimeout),
                    TimeSpan.FromMilliseconds(config.ConnectionTimeout),
                    config.ReadBufferSize
                    ),
                    serviceScope.ServiceProvider,
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

        private async Task ClientConnectionListeningProc(MiniWebServerBindingConfiguration binding, CancellationToken cancellationToken)
        {
            var listener = new TcpListener(binding.HttpEndPoint);
            listener.Start();

            logger.LogInformation("Server started on {binding}({https})", binding.HttpEndPoint, binding.Certificate != null ? "HTTPS" : "HTTP");

            while (running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);

                    var connectionId = Interlocked.Increment(ref nextClientId); // this function can be called concurrently (or not?) so we cannot use ++

                    logger.LogDebug("New client connected! ClientID = {cid}", connectionId);

                    Task t = HandleNewClientConnectionAsync(connectionId, binding, client);

                    if (!clientTasks.TryAdd(connectionId, t))
                    {
                        // hope this will never happen
                        logger.LogError("Ooops! Cannot add task to client task list");
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Server socket stopped listening on {binding}", binding.HttpEndPoint);
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error accepting client socket on {binding}", binding.HttpEndPoint);
                    break;
                }
            }

            listener.Stop();

            Task.WaitAll([.. clientTasks.Values], TimeSpan.FromSeconds(30));
        }

        private static void CloseConnection(TcpClient tcpClient)
        {
            tcpClient.Close();
            tcpClient.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting web server...");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            running = true;
            var clientConnectionListeningProcs = new List<Task>();

            foreach (var binding in config.Bindings)
            {
                clientConnectionListeningProcs.Add(ClientConnectionListeningProc(binding, stoppingToken));
            }

            try
            {
                Task.WaitAll([.. clientConnectionListeningProcs], stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }

            return Task.CompletedTask;
        }

        public Task Start(CancellationToken? cancellationToken = null)
        {
            if (config.Bindings.Count == 0)
            {
                throw new InvalidOperationException("No binding settings found");
            }

            return ExecuteAsync(cancellationToken ?? CancellationToken.None);
        }

        public void Stop(CancellationToken? cancellationToken = null)
        {
            running = false;

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
        }
    }
}