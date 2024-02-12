using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi.Parsers
{
    internal class CgiResponseStreamReader : ICgiResponseReader
    {
        private readonly StreamReader reader;
        private readonly CgiResponseStreamReaderOptions options;
        private readonly CancellationToken cancellationToken;
        private readonly ILogger logger;

        public CgiResponseStreamReader(StreamReader reader, CgiResponseStreamReaderOptions options, CancellationToken cancellationToken, ILogger logger) { 
            this.reader = reader;
            this.options = options;
            this.cancellationToken = cancellationToken;
            this.logger = logger;
        }
        public async Task<CgiResponse?> ReadAsync()
        {
            // try to read first line
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(options.ScriptMaxRunningTimeInMs);

            var localCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);
            var localCancellationToken = localCancellationTokenSource.Token;

            // https://datatracker.ietf.org/doc/html/rfc3875.html#section-6
            var line = await reader.ReadLineAsync(localCancellationToken);
            if (line != null)
            {
                var responseHeader = ParseFirstLine(line); // it must have at least one line
                if (responseHeader != null)
                {
                    if (responseHeader.CgiResponseType == CgiResponseTypes.DocumentResponse)
                    {
                        string cgiOutput = await reader.ReadToEndAsync();

                        var response = new CgiResponse()
                        {
                            ResponseCode = Abstractions.HttpResponseCodes.OK,
                            Content = new MiniApp.Content.StringContent(cgiOutput),
                            Headers = []
                        };

                        return response;
                    }
                }
            }
            else
            {
                logger.LogError("No data returned");
            }

            return new CgiResponse()
            {
                ResponseCode = Abstractions.HttpResponseCodes.InternalServerError,
                Content = MiniApp.Content.StringContent.Empty,
                Headers = []
            };
        }

        private CgiResponseResponseHeader? ParseFirstLine(string line)
        {
            var buffer = line.AsSpan();
            var responseHeader = new CgiResponseResponseHeader();

            if (buffer.StartsWith("Content-Type: ", StringComparison.OrdinalIgnoreCase))
            {
                responseHeader.CgiResponseType = CgiResponseTypes.DocumentResponse;
                responseHeader.ContentType = new string(buffer[14..]);
            }
            else
            {
                logger.LogError("Unsupported response type: {l}", line);
            }

            return responseHeader;
        }
    }
}
