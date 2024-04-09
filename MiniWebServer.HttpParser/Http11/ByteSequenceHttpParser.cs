using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using System.Buffers;
using System.Text;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.HttpParser.Http11
{
    // we will migrate code to this, working on RegEx is much slower and also hard to keep it safe (because we cannot control what clients send to us)
    public class ByteSequenceHttpParser : IHttpComponentParser
    {
        private readonly ILogger<ByteSequenceHttpParser> logger;
        private static readonly Dictionary<HttpMethod, byte[]> supportedMethodBytes = new() {
            {
                HttpMethod.Get,
                Encoding.ASCII.GetBytes("GET")
            },
            {
                HttpMethod.Post,
                Encoding.ASCII.GetBytes("POST")
            },
            {
                HttpMethod.Head,
                Encoding.ASCII.GetBytes("HEAD")
            },
            {
                HttpMethod.Put,
                Encoding.ASCII.GetBytes("PUT")
            },
            {
                HttpMethod.Delete,
                Encoding.ASCII.GetBytes("DELETE")
            },
            {
                HttpMethod.Connect,
                Encoding.ASCII.GetBytes("CONNECT")
            },
            {
                HttpMethod.Options,
                Encoding.ASCII.GetBytes("OPTIONS")
            },
            {
                HttpMethod.Trace,
                Encoding.ASCII.GetBytes("TRACE")
            }
        };
        private static readonly byte[] HTTP1_1_Bytes = Encoding.ASCII.GetBytes("HTTP/1.1");
        private static readonly byte[] HTTP1_1_CR_Bytes = Encoding.ASCII.GetBytes("HTTP/1.1\r");
        private readonly int maxUrlPartLength;

        public ByteSequenceHttpParser(ILoggerFactory? loggerFactory = default, int maxUrlPartLength = -1)
        {
            // note that maxUrlPartLength includes all url parts (url, hash, query string)
            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger<ByteSequenceHttpParser>();
            }
            else
            {
                logger = new NullLogger<ByteSequenceHttpParser>();
            }

            this.maxUrlPartLength = maxUrlPartLength <= 0 ? 8192 : maxUrlPartLength; // 8192 = 8KB
        }

        // in any case, if this function returns null, server should return a 400 Bad Request
        public virtual HttpRequestLine? ParseRequestLine(ReadOnlySequence<byte> buffer)
        {
            SequencePosition? pos = buffer.PositionOf((byte)' '); // there are 2 SPs in a request line

            if (pos.HasValue)
            {
                HttpMethod? httpMethod = null;
                var methodBytes = buffer.Slice(0, pos.Value);
                if (methodBytes.Length < 3)
                {
                    logger.LogDebug("Method too short");
                    return null;
                }
                else if (methodBytes.Length > 7)
                {
                    logger.LogDebug("Method too long");
                    return null;
                }

                var span = methodBytes.FirstSpan;
                foreach (var supportedHeader in supportedMethodBytes)
                {
                    if (span.SequenceCompareTo(supportedHeader.Value) == 0)
                    {
                        httpMethod = supportedHeader.Key;
                        break;
                    }
                }
                if (httpMethod == null)
                {
                    logger.LogDebug("Not supported method");
                    return null;
                }

                buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));

                // start parsing Url
                pos = buffer.PositionOf((byte)' '); // find the next SP
                if (!pos.HasValue)
                {
                    logger.LogDebug("2nd SP required");
                    return null;
                }

                if (buffer.Length > maxUrlPartLength + 11) // 11 = length of SP HTTP/1.1 CR LF
                {
                    logger.LogDebug("Url part too long");
                    return null;
                }

                if (TryParseUrl(buffer.Slice(0, pos.Value), out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters) && !string.IsNullOrEmpty(url)) // url cannot be missing
                {
                    buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));

                    // remove last CR
                    var ba = buffer.ToArray();
                    if (HTTP1_1_CR_Bytes.SequenceEqual(ba) || HTTP1_1_Bytes.SequenceEqual(ba)) // todo: can we use memory pool instead of ToArray?
                    {
                        HttpRequestLine requestLine = new(
                            httpMethod,
                            url,
                            hash ?? string.Empty,
                            queryString ?? string.Empty,
                            new HttpProtocolVersion("1", "1"),
                            segments ?? [],
                            parameters ?? []
                            );

                        return requestLine;
                    }
                    else
                    {
                        logger.LogDebug("HTTP version not supported");
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                logger.LogDebug("1st SP not found");
                return null;
            }
        }

        public virtual HttpHeader? ParseHeaderLine(ReadOnlySequence<byte> buffer)
        {
            SequencePosition? pos = buffer.PositionOf((byte)':'); 

            if (pos.HasValue)
            {
                var headerNameBuffer = buffer.Slice(0, pos.Value);
                string headerValue = string.Empty;

                buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));
                if (!buffer.IsEmpty)
                {
                    if (buffer.FirstSpan[0] == ' ') // since buffer is not empty, we always have at least 1 byte 
                    {
                        buffer = buffer.Slice(buffer.GetPosition(1)); // take the part after ' '
                    }
                }

                if (!buffer.IsEmpty)
                {
                    headerValue = Encoding.ASCII.GetString(buffer).Replace("\r", ""); // todo: find a way to replace before GetString()
                }

                return new HttpHeader(Encoding.ASCII.GetString(headerNameBuffer), headerValue);
            }

            return null;
        }

        public bool TryParseUrl(ReadOnlySequence<byte> readOnlySequence, out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters)
        {
            var s = "https://f" + Encoding.ASCII.GetString(readOnlySequence); // hack: add a faked scheme and host to make it an absolute uri
            var uri = new Uri(s);

            if (uri.AbsolutePath.Contains(".."))
            {
                url = null;
                hash = null;
                queryString = null;
                segments = null;
                parameters = null;

                return false;
            }

            url = uri.AbsolutePath;
            hash = uri.Fragment;
            queryString = Uri.UnescapeDataString(uri.Query);
            segments = uri.Segments;

            if (TryParseParameters(queryString, out HttpParameters? ps) && ps != null)
            {
                parameters = ps;
            }
            else
            {
                parameters = null;
                return false;
            }

            return true;
        }

        private static bool TryParseParameters(string queryString, out HttpParameters? parameters)
        {
            ArgumentNullException.ThrowIfNull(queryString);

            var query = queryString.AsMemory();
            if (!query.IsEmpty && query.Span[0] == '?') // remove first ?
                query = query[1..];

            parameters = [];

            while (!query.IsEmpty)
            {
                var ampIdx = query.Span.IndexOf('&');
                ReadOnlyMemory<char> segment;

                if (ampIdx == -1)
                {
                    // this is the only parameter left
                    segment = query;
                    query = default;
                }
                else
                {
                    segment = query[..ampIdx];
                    query = query[(ampIdx + 1)..];
                }

                var equalIdx = segment.Span.IndexOf('=');
                if (equalIdx == -1)
                {
                    if (!segment.IsEmpty)
                    {
                        parameters.Add(new HttpParameter(new string(segment.Span), string.Empty));
                    }
                }
                else
                {
                    var name = segment[..equalIdx];
                    var value = segment[(equalIdx + 1)..];

                    parameters.Add(new HttpParameter(new string(name.Span), new string(value.Span)));
                }
            }

            return true;
        }
    }
}
