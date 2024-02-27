using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.Http.Header;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi.Parsers
{
    internal class CgiResponseStreamReader(StreamReader reader, CgiResponseStreamReaderOptions options, ILogger logger, CancellationToken cancellationToken) : ICgiResponseReader
    {
        private readonly StreamReader reader = reader;
        private readonly CgiResponseStreamReaderOptions options = options;
        private readonly CancellationToken cancellationToken = cancellationToken;
        private readonly ILogger logger = logger;

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
                        var headerLines = await ReadHeaderLines(reader, localCancellationToken);
                        string cgiOutput = await reader.ReadToEndAsync();

                        var response = new CgiResponse()
                        {
                            ResponseCode = Abstractions.HttpResponseCodes.OK,
                            Content = new MiniApp.Content.StringContent(cgiOutput),
                            Headers = new(headerLines)
                        };

                        return response;
                    }
                    else if (responseHeader.CgiResponseType == CgiResponseTypes.ClientRedirectResponse)
                    {
                        var headerLines = await ReadHeaderLines(reader, localCancellationToken);

                        var response = new CgiResponse()
                        {
                            ResponseCode = responseHeader.ResponseCode,
                            Content = MiniApp.Content.StringContent.Empty,
                            Headers = new(headerLines)
                        };

                        return response;
                    }
                    else return new CgiResponse()
                    {
                        ResponseCode = Abstractions.HttpResponseCodes.InternalServerError,
                        Content = MiniApp.Content.StringContent.Empty,
                        Headers = []
                    };
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

        private static async Task<IEnumerable<HttpHeader>> ReadHeaderLines(StreamReader reader, CancellationToken localCancellationToken)
        {
            var line = await reader.ReadLineAsync(localCancellationToken);
            var headers = new List<HttpHeader>();

            while (!string.IsNullOrEmpty(line))
            {
                var header = ParseHeaderLine(line);

                if (header != null)
                { 
                    headers.Add(header);
                }
            }

            return headers;
        }

        public static HttpHeader? ParseHeaderLine(string line)
        {
            int pos = line.IndexOf(':'); // there are 2 SPs in a request line

            if (pos > 0)
            {
                var header = line[1..(pos + 1)];
                while (pos < header.Length && header[pos] == ' ')
                {
                    pos++;
                }

                return new HttpHeader(
                    header, 
                    line[pos..]
                    );
            }

            return null;
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
            else if (buffer.StartsWith("Location: ", StringComparison.OrdinalIgnoreCase))
            {
                responseHeader.CgiResponseType = CgiResponseTypes.ClientRedirectResponse;
                responseHeader.Location = new string(buffer[10..]);
            }
            else if (buffer.StartsWith("Status: ", StringComparison.OrdinalIgnoreCase))
            {
                responseHeader.CgiResponseType = CgiResponseTypes.DocumentResponse;
                int idx = buffer.IndexOf(' ');
                if (idx > 0)
                {
                    if (int.TryParse(buffer[1..(idx)], out var status))
                    {
                        responseHeader.ResponseCode = (Abstractions.HttpResponseCodes)status;
                        responseHeader.ReasonPhrase = new string(buffer[(idx + 1)..]);
                    }
                    else
                    {
                        logger.LogError("Invalid CGI response: status must be an integer");
                        return null;
                    }
                }
                else
                {
                    if (int.TryParse(buffer, out var status))
                    {
                        responseHeader.ResponseCode = (Abstractions.HttpResponseCodes)status;
                        responseHeader.ReasonPhrase = string.Empty;
                    }
                    else
                    {
                        logger.LogError("Invalid CGI response: status must be an integer");
                        return null;
                    }
                }
            }
            else
            {
                logger.LogError("Unsupported response type: {l}", line);
            }

            return responseHeader;
        }
    }
}
