using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;

namespace MiniWebServer.Cgi
{
    public class CgiMiddleware : IMiddleware
    {
        private CgiOptions options;
        private readonly ILogger logger;

        public CgiMiddleware(CgiOptions options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            if (options.Handlers.Length == 0)
            {
                // there's nothing to do

                await next.InvokeAsync(context, cancellationToken);
                return;
            }

            var matchedHander = options.Handlers.Where(h => context.Request.Url.StartsWith(h.Route)).FirstOrDefault();

            if (matchedHander != null)
            {
                try
                {
                    await ExecuteCgiHandler(matchedHander, context, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing Cgi handler");
                    context.Response.StatusCode = HttpResponseCodes.InternalServerError;

                    return;
                }
            }
            else
            {
                await next.InvokeAsync(context, cancellationToken);
            }
        }

        private async Task ExecuteCgiHandler(CgiHandler matchedHandler, IMiniAppRequestContext context, CancellationToken cancellationToken)
        {
            var exeFile = new FileInfo(matchedHandler.Executable);
            if (!exeFile.Exists)
            {
                throw new FileNotFoundException("Handler not found", matchedHandler.Executable);
            }

            if (!Directory.Exists(matchedHandler.WorkingDirectory))
            {
                throw new DirectoryNotFoundException($"Working directory not found: {matchedHandler.WorkingDirectory}");
            }

            var environmentVariables = BuildEnvironmentVariables(matchedHandler, context);

            using Process handlerProcess = new();
            handlerProcess.StartInfo.FileName = matchedHandler.Executable;
            handlerProcess.StartInfo.WorkingDirectory = matchedHandler.WorkingDirectory;
            handlerProcess.StartInfo.UseShellExecute = false;
            handlerProcess.StartInfo.RedirectStandardInput = true;
            handlerProcess.StartInfo.RedirectStandardOutput = true;
            // TODO: redirect standard error to server's log (or to response?)

            foreach (var variable in environmentVariables)
            {
                handlerProcess.StartInfo.Environment.Add(variable.Key, variable.Value);
            }

            // TODO: don't forget to use another user context

            handlerProcess.Start();

            using var cgiInputStreamWriter = handlerProcess.StandardInput;
            // read data from request body and send to handler's standard input
            await WriteBodyToHandlerInputAsync(context, cgiInputStreamWriter, cancellationToken);
            cgiInputStreamWriter.Close();

            // then read handler's standard output to write to response
            using var cgiOutputStreamReader = handlerProcess.StandardOutput;
            string cgiOutput = await cgiOutputStreamReader.ReadToEndAsync();
            cgiOutputStreamReader.Close();

            handlerProcess.WaitForExit();

            context.Response.Content = new MiniApp.Content.StringContent(cgiOutput);
            context.Response.StatusCode = HttpResponseCodes.OK;
        }

        private static async Task WriteBodyToHandlerInputAsync(IMiniAppRequestContext context, StreamWriter cgiInputStreamWriter, CancellationToken cancellationToken)
        {
            var contentLength = context.Request.ContentLength;
            if (contentLength > 0)
            {
                var reader = context.Request.BodyManager.GetReader() ?? throw new InvalidOperationException("Body reader cannot be null");
                var encoding = Encoding.UTF8; // todo: we should take the right encoding from Content-Type

                ReadResult readResult = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                long bytesRead = 0;
                while (bytesRead < contentLength)
                {
                    long maxBytesToRead = contentLength - bytesRead;
                    if (buffer.Length >= maxBytesToRead)
                    {
                        await cgiInputStreamWriter.WriteAsync(encoding.GetString(buffer.Slice(0, maxBytesToRead)).AsMemory(), cancellationToken); // what will happen if a multi-byte character is partly sent?

                        reader.AdvanceTo(buffer.GetPosition(maxBytesToRead));
                        break;
                    }
                    else if (buffer.Length > 0)
                    {
                        await cgiInputStreamWriter.WriteAsync(encoding.GetString(buffer).AsMemory(), cancellationToken);
                        reader.AdvanceTo(buffer.GetPosition(buffer.Length));

                        bytesRead += buffer.Length;
                    }

                    readResult = await reader.ReadAsync(cancellationToken);
                    buffer = readResult.Buffer;
                }
            }
        }

        private static IDictionary<string, string?> BuildEnvironmentVariables(CgiHandler matchedHander, IMiniAppRequestContext context)
        {
            var environmentVariables = new Dictionary<string, string?>();

            environmentVariables.Add("CONTENT_LENGTH", context.Request.ContentLength.ToString());
            environmentVariables.Add("CONTENT_TYPE", context.Request.ContentType);

            return environmentVariables;
        }
    }
}
