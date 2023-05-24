using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Server.Abstractions;
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
        private readonly ServiceProvider serviceProvider;
        private readonly IProtocolHandlerFactory protocolHandlerFactory;
        private readonly IDictionary<string, Host.Host> hostContainers;
        private readonly ILogger<MiniWebServer> logger;
        private TcpListener? server;
        private bool running;
        private int nextClientId = 1;
        private bool disposed = false;

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

        public Task Start()
        {
            logger.LogInformation("Starting web server...");

            if (config.Certificate != null)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            }

            running = true;

            return ClientConnectionListeningProc();
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
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        RemoteCertificateValidationCallback = ValidateClientCertificate
                    };

                    sslStream.AuthenticateAsServer(options);

                    stream = sslStream;
                }

                var client = new MiniWebClientConnection(
                    new MiniWebConnectionConfiguration(
                        nextClientId++,
                        tcpClient,
                        stream,
                        protocolHandlerFactory.Create(
                        new ProtocolHandlerConfiguration(ProtocolHandlerFactory.HTTP11, config.MaxRequestBodySize)
                    ), // A connection always starts with HTTP 1.1 
                    hostContainers,
                    TimeSpan.FromMilliseconds(config.ConnectionTimeout),
                    config.ReadBufferSize
                    ),
                    serviceProvider,
                    cancellationToken
                );

                logger.LogInformation("New client connected {tcpClient}", tcpClient.Client.RemoteEndPoint);
                await client.HandleRequestAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accepting client");
                CloseConnection(tcpClient);
            }
        }

        private bool ValidateClientCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            logger.LogInformation("Validating certificate: {certificate}", certificate);

            return true; // accept all :D
        }

        public void Stop()
        {
            running = false;
            server?.Stop();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
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
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error accepting client socket");
                    break;
                }
            }
        }

        private static void CloseConnection(TcpClient tcpClient)
        {
            tcpClient.Close();
            tcpClient.Dispose();
        }

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