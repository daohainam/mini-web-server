using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

namespace MiniWebServer.Cgi
{
    public class CgiMiddleware : IMiddleware
    {
        private const string GATEWAY_INTERFACE = "CGI/1.1";

        private readonly CgiOptions options;
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

            if (!Directory.Exists(matchedHandler.ScriptDirectory))
            {
                throw new DirectoryNotFoundException($"Script directory not found: {matchedHandler.ScriptDirectory}");
            }

            string scriptFilePath = BuildScriptFilePath(matchedHandler.ScriptDirectory, context.Request.Url);
            if (!System.IO.File.Exists(scriptFilePath))
            {
                throw new FileNotFoundException("Script file not found", scriptFilePath);
            }

            var environmentVariables = BuildEnvironmentVariables(matchedHandler, context);

            using Process handlerProcess = new();
            handlerProcess.StartInfo.FileName = matchedHandler.Executable;
            handlerProcess.StartInfo.WorkingDirectory = matchedHandler.WorkingDirectory;
            handlerProcess.StartInfo.UseShellExecute = false;
            handlerProcess.StartInfo.RedirectStandardInput = true;
            handlerProcess.StartInfo.RedirectStandardOutput = true;

            handlerProcess.StartInfo.ArgumentList.Add(scriptFilePath);
            if (matchedHandler.Parameters.Length > 0)
            {
                foreach (var parameter in matchedHandler.Parameters)
                {
                    handlerProcess.StartInfo.ArgumentList.Add(parameter);
                }
            }

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

        private static string BuildScriptFilePath(string scriptDirectory, string url)
        {
            int idx = 0; // we skip beginning slashes
            while (idx < url.Length && url[idx] == '/')
            {
                idx++;
            }

            if (idx == url.Length)
            {
                return Path.Combine(scriptDirectory, string.Empty);
            }
            else
            {
                return Path.Combine(scriptDirectory, url[idx..].Replace('/', Path.DirectorySeparatorChar));
            }
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
            /* https://datatracker.ietf.org/doc/html/draft-robinson-www-interface-00#section-5
            here is the list of environment variables, implemented ones have a (*) 
                AUTH_TYPE
                ONTENT_LENGTH (*) 
                CONTENT_TYPE (*) 
                GATEWAY_INTERFACE (*) 
                HTTP_* (*)
                PATH_INFO (*)
                PATH_TRANSLATED (*) skip this for security purpose
                QUERY_STRING (*)
                REMOTE_ADDR (*)
                REMOTE_HOST (*)
                REMOTE_IDENT (*)
                REMOTE_USER (*)
                REQUEST_METHOD (*)
                SCRIPT_NAME
                SERVER_NAME (*)
                SERVER_PORT (*)
                SERVER_PROTOCOL (*)
                SERVER_SOFTWARE (*)
            */

            var environmentVariables = new Dictionary<string, string?>
            {
                { "CONTENT_LENGTH", context.Request.ContentLength.ToString() },
                { "CONTENT_TYPE", context.Request.ContentType },
                { "GATEWAY_INTERFACE", GATEWAY_INTERFACE }
            };

            AddHttpVariables(context, environmentVariables); // HTTP_*

            environmentVariables.Add("PATH_INFO", context.Request.Url);
            // we don't provide PATH_TRANSLATED
            environmentVariables.Add("PATH_TRANSLATED", string.Empty);
            environmentVariables.Add("QUERY_STRING", context.Request.QueryString);
            environmentVariables.Add("REMOTE_ADDR", context.Request.RemoteAddress?.ToString() ?? string.Empty);
            environmentVariables.Add("REMOTE_HOST", string.Empty);
            environmentVariables.Add("REMOTE_IDENT", string.Empty);
            environmentVariables.Add("REMOTE_USER", string.Empty);
            environmentVariables.Add("REQUEST_METHOD", context.Request.Method.Method);
            environmentVariables.Add("SERVER_NAME", context.Request.Host);
            environmentVariables.Add("SERVER_PORT", context.Request.Port.ToString());
            environmentVariables.Add("SERVER_PROTOCOL", $"HTTP/{context.Request.HttpVersion}");
            environmentVariables.Add("SERVER_SOFTWARE", "Mini-Web-Server");


            return environmentVariables;
        }

        private static void AddHttpVariables(IMiniAppRequestContext context, Dictionary<string, string?> environmentVariables)
        {

            var httpVarName = new StringBuilder();
            var httpVarValue = new StringBuilder();
            foreach (var header in context.Request.Headers)
            {
                /*
                The HTTP header name is converted to upper case, has all
                occurrences of "-" replaced with "_" and has "HTTP_" prepended to
                give the environment variable name.
                 */

                // we can simply use ToUpper and Replace, but they don't have a good performance
                httpVarName.Clear();
                httpVarName.Append("HTTP_");
                for (int i = 0; i < header.Key.Length; i++)
                {
                    var c = header.Key[i];
                    if (c == '-')
                    {
                        httpVarName.Append('_');
                    }
                    else
                    {
                        httpVarName.Append(char.ToUpper(c));
                    }
                }

                httpVarValue.Clear();
                foreach (var headerValue in header.Value)
                {
                    if (httpVarValue.Length > 0)
                    {
                        httpVarValue.Append(';');
                    }

                    for (int i = 0; i < headerValue.Length; i++)
                    {
                        var c = headerValue[i];
                        if (c < 0x20) // skip invalid characters
                        {
                            // do nothing
                        }
                        else
                        {
                            httpVarValue.Append(c);
                        }
                    }
                }

                var sHttpVarName = httpVarName.ToString();
                if (environmentVariables.TryGetValue(sHttpVarName, out var value))
                {
                    var sb = new StringBuilder(value);
                    sb.Append(';');
                    sb.Append(httpVarValue);

                    environmentVariables[sHttpVarName] = sb.ToString();
                }
                else
                {
                    environmentVariables.Add(sHttpVarName, httpVarValue.ToString());
                }
            }
        }
    }
}
