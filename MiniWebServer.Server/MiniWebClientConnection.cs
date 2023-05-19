using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.Http.Helpers;
using MiniWebServer.Server.MiniApp;
using System.Buffers;
using System.Net.Sockets;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server
{
    public class MiniWebClientConnection
    {
        public const int BufferSize = 1024 * 64;
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
            int id,
            TcpClient tcpClient,
            Stream clientStream,
            IProtocolHandler connectionHandler,
            IDictionary<string, Host.Host> hostContainers,
            TimeSpan executeTimeout,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            Id = id;
            TcpClient = tcpClient;
            ClientStream = clientStream;
            ProtocolHandler = connectionHandler;
            this.hostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            ExecuteTimeout = executeTimeout;
            this.cancellationToken = cancellationToken;
            this.logger = logger;

            ProtocolHandlerData = new ProtocolHandlerData();
            requestBuilder = new HttpWebRequestBuilder();
            responseBuilder = new HttpWebResponseBuilder();
        }

        public int Id { get; }
        public TcpClient TcpClient { get; }
        public Stream ClientStream { get; }
        public IProtocolHandler ProtocolHandler { get; }
        public readonly IDictionary<string, Host.Host> hostContainers;

        public ProtocolHandlerData ProtocolHandlerData { get; }
        public TimeSpan ExecuteTimeout { get; }

        private readonly CancellationToken cancellationToken;
        private readonly ILogger logger;

        private readonly IHttpRequestBuilder requestBuilder;
        private readonly IHttpResponseBuilder responseBuilder;
        private readonly ProtocolHandlerData protocolHandlerData = new(); // protocol handler uses this to hold it's own state data

        public async Task HandleRequestAsync()
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken); // we will use this to keep control on connection timeout
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            bool isKeepAlive = true;


            try
            {
                // allocate buffers

                while (isKeepAlive)
                {
                    if (!await ReadRequestAsync(cancellationToken))
                    {
                        isKeepAlive = false; // we always close wrongly working connections
                        return;
                    }
                    else
                    {
                        isKeepAlive = false;

                        var request = requestBuilder.Build();

                        var app = FindApp(request);

                        if (app != null)
                        {
                            await ExecuteCallableAsync(request, app, cancellationToken);
                        }

                        var response = responseBuilder.Build();
                        await SendResponseAsync(response, cancellationToken);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request");
            }
        }

        private async Task<bool> ReadRequestAsync(CancellationToken cancellationToken)
        {
            bool succeed = false;

            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(BufferSize); // rfc9112: we should have ability to process request line length at minimum 8000 octets
            var buffer = owner.Memory;

            try
            {
                while (true)
                {
                    var bytesRead = await ClientStream.ReadAsync(buffer, cancellationToken);
                    var readBuffer = buffer[..bytesRead];

                    int bytesProcessed = 0;

                    var readRequestResult = ProtocolHandler.ReadRequest(readBuffer[bytesProcessed..].Span, requestBuilder, protocolHandlerData, out bytesProcessed);
                    if (readRequestResult == ProtocolHandlerStates.BuildRequestStates.InProgress)
                    {
                        while (readRequestResult == ProtocolHandlerStates.BuildRequestStates.InProgress && bytesProcessed < readBuffer.Length)
                        {
                            readRequestResult = ProtocolHandler.ReadRequest(readBuffer[bytesProcessed..].Span, requestBuilder, protocolHandlerData, out int bp);

                            bytesProcessed += bp;
                        }
                    }

                    if (readRequestResult == ProtocolHandlerStates.BuildRequestStates.Succeeded)
                    {
                        succeed = true;
                        break;
                    }
                    else
                    {
                        // failed
                        break;
                    }
                }
            }
            finally
            {
                owner.Dispose();
            }

            return succeed;
        }

        private IMiniApp? FindApp(HttpRequest request)
        {
            string host = request.Headers.Host;
            if (string.IsNullOrEmpty(host))
            {
                return null;
            }

            if (hostContainers.TryGetValue(host, out var container))
            {
                return container.App;
            }
            else
            {
                if (hostContainers.TryGetValue(string.Empty, out container)) // Host "" is a catch-all host
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
                await CallByMethod(app, request, responseBuilder);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling resource");
            }
        }

        private void CloseConnection()
        {
            TcpClient.GetStream().Flush();
            TcpClient.Close();
        }


        private async Task SendResponseAsync(HttpResponse response, CancellationToken cancellationToken)
        {
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(BufferSize); // rfc9112: we should have ability to process request line length at minimum 8000 octets
            var buffer = owner.Memory;

            var state = ProtocolHandler.WriteResponse(buffer.Span, response, protocolHandlerData, out int bytesProcessed);
            if (state == ProtocolHandlerStates.WriteResponseStates.InProgress)
            {
                if (bytesProcessed > 0)
                {
                     await ClientStream.WriteAsync(buffer[..bytesProcessed], cancellationToken);
                }
            }

            while (state == ProtocolHandlerStates.WriteResponseStates.InProgress)
            {
                state = ProtocolHandler.WriteResponse(buffer.Span, response, protocolHandlerData, out bytesProcessed);

                if (bytesProcessed > 0)
                {
                    await ClientStream.WriteAsync(buffer[..bytesProcessed], cancellationToken);
                }
            }
        }

        private async Task CallByMethod(IMiniApp app, HttpRequest httpRequest, IHttpResponseBuilder responseBuilder)
        {
            try
            {
                var context = BuildMiniContext(app, httpRequest);
                var request = BuildMiniAppRequest(context, httpRequest);
                var response = BuildMiniAppResponse(context, responseBuilder);

                if (httpRequest.Method == HttpMethod.Get)
                {
                    await app.Get(request, response);
                }
                else
                {
                    StandardResponseBuilderHelpers.MethodNotAllowed(responseBuilder);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing resource");
            }
        }

        private MiniContext BuildMiniContext(IMiniApp app, HttpRequest httpRequest)
        {
            return new MiniContext(app);
        }

        private MiniRequest BuildMiniAppRequest(MiniContext context, HttpRequest httpRequest)
        {
            var request = new MiniRequest(context, httpRequest);

            return request;
        }

        private MiniResponse BuildMiniAppResponse(MiniContext context, IHttpResponseBuilder responseBuilder)
        {
            var response = new MiniResponse(context, responseBuilder);

            return response;
        }

    }
}
