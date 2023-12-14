using MiniWebServer.Abstractions.Http.Header;
using MiniWebServer.Abstractions.Http.Header.Parsers;
using System.Net;
using System.Reflection.PortableExecutable;

namespace MiniWebServer.Abstractions.Http
{
    // here is the this of standard request headers defined in 
    // TODO: using TryGetValueAsString is not a good practice, we should have a better/faster way to store headers (cache to separate fields)
    public class HttpRequestHeaders : HttpHeaders
    {
        private readonly Dictionary<string, object> parsedValues = new(StringComparer.OrdinalIgnoreCase);
        private AuthorizationHeader? authorizationHeader;
        private RangeHeader? rangeHeader;
        private string connectionHeader = string.Empty;

        private static readonly Dictionary<string, Action<HttpHeader, HttpRequestHeaders>> headerParsers = new(StringComparer.OrdinalIgnoreCase);

        static HttpRequestHeaders()
        {
            headerParsers.Add(HttpHeaderNames.Connection, (header, httpRequestHeaders) => {
                httpRequestHeaders.connectionHeader = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.Authorization, (header, httpRequestHeaders) => {
                if (AuthorizationHeaderParser.TryParse(header.Value.FirstOrDefault(), out var authorization))
                {
                    httpRequestHeaders.authorizationHeader = authorization;
                }
            });
            headerParsers.Add(HttpHeaderNames.Range, (header, httpRequestHeaders) => {
                if (RangeHeaderParser.TryParse(header.Value.FirstOrDefault(), out var range))
                {
                    httpRequestHeaders.rangeHeader = range;
                }
            });
        }

        public HttpRequestHeaders()
        {
            HeaderAdded += HttpRequestHeaders_AddedOrModified;
            HeaderChanged += HttpRequestHeaders_AddedOrModified;
            HeaderRemoved += HttpRequestHeaders_HeaderRemoved;
        }
        public HttpRequestHeaders(string name, string value) : this()
        {
            Add(name, value);
        }

        public HttpRequestHeaders(params HttpHeader[] headers) : this()
        {
            foreach (var header in headers)
            {
                Add(header);
            }
        }

        private void HttpRequestHeaders_HeaderRemoved(HttpHeader header)
        {
            parsedValues.Remove(HttpHeaderNames.Range);
        }

        private void HttpRequestHeaders_AddedOrModified(HttpHeader header)
        {
            if (header == null)
                return;

            //if (header.Name == HttpHeaderNames.Range)
            //{
            //    var value = header.Value.FirstOrDefault();
            //    if (value != null && RangeHeader.TryParse(value, out var range))
            //    {
            //        this.Range = range;
            //    }
            //    else
            //    {
            //        throw new InvalidOperationException("Invalid Range header");
            //    }
            //}

            if (headerParsers.TryGetValue(header.Name, out var handler))
            {
                handler(header, this);
            }

            //if (headerParsers.TryGetValue(header.Name, out IHeaderParser? parser))
            //{
            //    var value = header.Value.FirstOrDefault();
            //    if (value != null && RangeHeader.TryParse(value, out var parsedValue) && parsedValue != null)
            //    {
            //        parsedValues.Add(header.Name, parsedValue);
            //    }
            //    else
            //    {
            //        throw new InvalidOperationException($"Unable to parse {header.Name} header, value: {string.Join(", ", header.Value)}");
            //    }
            //}
        }

        public string AcceptLanguage
        {
            get
            {
                return TryGetValueAsString("Accept-Language");
            }
        }
        public AuthorizationHeader? Authorization
        {
            get
            {
                return authorizationHeader;
            }
        }
        public string CacheControl
        {
            get
            {
                return TryGetValueAsString("Cache-Control");
            }
        }
        public string Connection
        {
            get
            {
                return connectionHeader;
            }
        }
        public string ContentType
        {
            get
            {
                return TryGetValueAsString("Content-Type");
            }
        }
        public string Host
        {
            get
            {
                return TryGetValueAsString("Host");
            }
        }
        public string Origin
        {
            get
            {
                return TryGetValueAsString("Origin");
            }
        }
        public string TransferEncoding
        {
            get
            {
                return TryGetValueAsString("Transfer-Encoding");
            }
        }

        public string[] AcceptEncoding
        {
            get
            {
                var value = TryGetValueAsString("Accept-Encoding");
                if (string.IsNullOrEmpty(value))
                {
                    return [];
                }
                // todo: need to support something like: br;q=1.0, gzip;q=0.8, *;q=0.1
                var values = value.Split(", ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return values;
            }
        }

        public RangeHeader? Range
        {
            get
            {
                return rangeHeader;
            }
        }

        public string SecWebSocketKey
        {
            get
            {
                return TryGetValueAsString("Sec-WebSocket-Key");
            }
        }
        public string SecWebSocketProtocol
        {
            get
            {
                return TryGetValueAsString("Sec-WebSocket-Protocol");
            }
        }
        public string SecWebSocketVersion
        {
            get
            {
                return TryGetValueAsString("Sec-WebSocket-Version");
            }
        }

        public string Upgrade
        {
            get
            {
                return TryGetValueAsString("Upgrade");
            }
        }

        private string TryGetValueAsString(string name, string defaultValue = "")
        {
            if (TryGetValue(name, out var value))
            {
                if (value == null)
                    return defaultValue;

                return value.Value.FirstOrDefault(defaultValue);
            }
            else
            {
                return defaultValue;
            }

        }
    }
}
