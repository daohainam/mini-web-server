using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
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
            MiniWebConnectionConfiguration config,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            this.config = config;
            this.cancellationToken = cancellationToken;
            this.logger = logger;

            requestBuilder = new HttpWebRequestBuilder();
            responseBuilder = new HttpWebResponseBuilder();
        }

        private readonly MiniWebConnectionConfiguration config;

        public int Id => config.Id;
        public TimeSpan ExecuteTimeout { get; }

        private readonly CancellationToken cancellationToken;
        private readonly ILogger logger;

        private readonly IHttpRequestBuilder requestBuilder;
        private readonly IHttpResponseBuilder responseBuilder;

        public async Task HandleRequestAsync()
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken); // we will use this to keep control on connection timeout
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            bool isKeepAlive = true;

            try
            {
                PipeReader requestPipeReader = PipeReader.Create(config.ClientStream);
                PipeWriter responsePipeWriter = PipeWriter.Create(config.ClientStream);

                // allocate buffers
                //Task fillRequestPipeTask = FillPipeFromStreamAsync(config.ClientStream, requestPipe.Writer, cancellationToken);
                //Task fillResponsePipeTask = FillStreamFromPipeAsync(config.ClientStream, responsePipe.Reader, cancellationToken);

                while (isKeepAlive)
                {
                    if (!await ReadRequestAsync(requestPipeReader, requestBuilder, cancellationToken))
                    {
                        isKeepAlive = false; // we always close wrongly working connections
                        break;
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
                        await SendResponseAsync(responsePipeWriter, response, cancellationToken);
                    }
                }

                //Task.WaitAll(fillRequestPipeTask, fillResponsePipeTask);
                CloseConnection();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request");
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
                logger.LogError(ex, "Error reading request");
            }

            return succeed;
        }

        private async Task SendResponseAsync(PipeWriter writer, HttpResponse response, CancellationToken cancellationToken)
        {
            await config.ProtocolHandler.WriteResponseAsync(writer, response, cancellationToken);

            await writer.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// send available data from pipeWriter to stream (socket's stream)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pipeWriter"></param>
        /// <returns></returns>
        //private async Task FillPipeFromStreamAsync(Stream stream, PipeWriter pipeWriter, CancellationToken cancellationToken)
        //{
        //    while (true)
        //    {
        //        Memory<byte> memory = pipeWriter.GetMemory(config.ReadRequestBufferSize);
        //        try
        //        {
        //            int bytesRead = await stream.ReadAsync(memory, cancellationToken);
        //            if (bytesRead == 0)
        //            {
        //                break;
        //            }
        //            // Tell the PipeWriter how much was read from the Socket.
        //            pipeWriter.Advance(bytesRead);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError(ex, "Error reading from socket");
        //            break;
        //        }


        //        FlushResult result = await pipeWriter.FlushAsync();
        //        if (result.IsCompleted)
        //        {
        //            break;
        //        }
        //    }

        //    // By completing PipeWriter, tell the PipeReader that there's no more data coming.
        //    await pipeWriter.CompleteAsync();
        //}

        /// <summary>
        /// send data from pipeReader to stream (socket's stream)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pipeReader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //private async Task FillStreamFromPipeAsync(Stream stream, PipeReader pipeReader, CancellationToken cancellationToken)
        //{
        //    while (true)
        //    {
        //        ReadResult result = await pipeReader.ReadAsync(cancellationToken);
        //        ReadOnlySequence<byte> buffer = result.Buffer;

        //        try
        //        {
        //            int bytesWrite = await stream.WriteAsync(buffer, cancellationToken);
        //            if (bytesWrite == 0)
        //            {
        //                break;
        //            }

        //            if (result.IsCompleted)
        //                break;

        //            pipeReader.AdvanceTo(buffer.Start, buffer.End);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError(ex, "Error writing to socket");
        //            break;
        //        }
        //    }

        //    await pipeReader.CompleteAsync();
        //}



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

        private async Task ExecuteCallableAsync(HttpRequest request, IMiniApp app, CancellationToken cancellationToken)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            try
            {
                await CallByMethod(app, request, responseBuilder, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling resource");
            }
        }

        private void CloseConnection()
        {
            config.TcpClient.GetStream().Flush();
            config.TcpClient.Close();
        }

        private async Task CallByMethod(IMiniApp app, HttpRequest httpRequest, IHttpResponseBuilder responseBuilder, CancellationToken cancellationToken)
        {
            try
            {
                var context = BuildMiniContext(app, httpRequest);
                var request = BuildMiniAppRequest(context, httpRequest);
                var response = BuildMiniAppResponse(context, responseBuilder);

                if (httpRequest.Method == HttpMethod.Get)
                {
                    await app.Get(request, response, cancellationToken);
                }
                else if (httpRequest.Method == HttpMethod.Post)
                {
                    await app.Post(request, response, cancellationToken);
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

        private static MiniContext BuildMiniContext(IMiniApp app, HttpRequest httpRequest)
        {
            return new MiniContext(app);
        }

        private static MiniRequest BuildMiniAppRequest(MiniContext context, HttpRequest httpRequest)
        {
            var request = new MiniRequest(context, httpRequest);

            return request;
        }

        private static MiniResponse BuildMiniAppResponse(MiniContext context, IHttpResponseBuilder responseBuilder)
        {
            var response = new MiniResponse(context, responseBuilder);

            return response;
        }

    }
}
