using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.Http.Header;
using MiniWebServer.Server.Abstractions.Parsers;
using MiniWebServer.Server.Cookie;
using MiniWebServer.Server.Http.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http
{
    public class HttpRequestHeadersFactory
    {
        private static readonly Dictionary<string, Action<HttpHeader, HttpRequestHeaders>> headerParsers = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Action<HttpHeader, HttpRequestHeaders>> headerRemovers = new(StringComparer.OrdinalIgnoreCase);

        private static readonly DefaultCookieParser CookieParser = new();

        static HttpRequestHeadersFactory()
        {
            headerParsers.Add(HttpHeaderNames.AcceptEncoding, (header, httpRequestHeaders) => {
                var value = header.Value.FirstOrDefault(string.Empty);
                if (string.IsNullOrEmpty(value))
                {
                    httpRequestHeaders.AcceptEncoding = [];
                }
                // todo: need to support something like: br;q=1.0, gzip;q=0.8, *;q=0.1
                var values = value.Split(", ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                httpRequestHeaders.AcceptEncoding = values;
            });
            headerParsers.Add(HttpHeaderNames.AcceptLanguage, (header, httpRequestHeaders) => {
                httpRequestHeaders.AcceptLanguage = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.Authorization, (header, httpRequestHeaders) => {
                if (AuthorizationHeaderParser.TryParse(header.Value.FirstOrDefault(), out var authorization))
                {
                    httpRequestHeaders.Authorization = authorization;
                }
            });
            headerParsers.Add(HttpHeaderNames.CacheControl, (header, httpRequestHeaders) => {
                httpRequestHeaders.CacheControl = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.Cookie, (header, httpRequestHeaders) => {
                var cookies = CookieParser.ParseCookieHeader(header.Value.FirstOrDefault(string.Empty));
                if (cookies == null)
                {
                    throw new InvalidHeaderException(header);
                }
                else
                {
                    foreach (var cookie in cookies)
                        httpRequestHeaders.Cookie.Add(cookie.Name, cookie);
                }
            });
            headerParsers.Add(HttpHeaderNames.Connection, (header, httpRequestHeaders) => {
                httpRequestHeaders.Connection = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.ContentType, (header, httpRequestHeaders) => {
                httpRequestHeaders.ContentType = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.ContentLength, (header, httpRequestHeaders) => {
                if (long.TryParse(header.Value.FirstOrDefault(string.Empty), out long length) && length >= 0)
                {
                    httpRequestHeaders.ContentLength = length;
                }
                else
                {
                    throw new InvalidHeaderException(header);
                }
            });
            headerParsers.Add(HttpHeaderNames.Host, (header, httpRequestHeaders) => {
                if (HostHeader.TryParse(header.Value.FirstOrDefault(string.Empty), out var host))
                {
                    httpRequestHeaders.Host = host;
                }
                else
                {
                    throw new InvalidHeaderException(header);
                }
            });
            headerParsers.Add(HttpHeaderNames.Origin, (header, httpRequestHeaders) => {
                httpRequestHeaders.Origin = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.Range, (header, httpRequestHeaders) => {
                if (RangeHeaderParser.TryParse(header.Value.FirstOrDefault(), out var range))
                {
                    httpRequestHeaders.Range = range;
                }
            });
            headerParsers.Add(HttpHeaderNames.SecWebSocketKey, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketKey = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.SecWebSocketProtocol, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketProtocol = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.SecWebSocketVersion, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketVersion = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.TransferEncoding, (header, httpRequestHeaders) => {
                httpRequestHeaders.TransferEncoding = header.Value.FirstOrDefault(string.Empty);
            });
            headerParsers.Add(HttpHeaderNames.Upgrade, (header, httpRequestHeaders) => {
                httpRequestHeaders.Upgrade = header.Value.FirstOrDefault(string.Empty);
            });

            // Header remover handlers
            headerRemovers.Add(HttpHeaderNames.AcceptEncoding, (header, httpRequestHeaders) => {
                httpRequestHeaders.AcceptEncoding = [];
            });
            headerRemovers.Add(HttpHeaderNames.AcceptLanguage, (header, httpRequestHeaders) => {
                httpRequestHeaders.AcceptLanguage = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.Authorization, (header, httpRequestHeaders) => {
                httpRequestHeaders.Authorization = null;
            });
            headerRemovers.Add(HttpHeaderNames.Cookie, (header, httpRequestHeaders) => {
                httpRequestHeaders.Cookie = [];
            });
            headerRemovers.Add(HttpHeaderNames.Connection, (header, httpRequestHeaders) => {
                httpRequestHeaders.Connection = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.ContentLength, (header, httpRequestHeaders) => {
                httpRequestHeaders.ContentLength = -1;
            });
            headerRemovers.Add(HttpHeaderNames.ContentType, (header, httpRequestHeaders) => {
                httpRequestHeaders.ContentType = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.Host, (header, httpRequestHeaders) => {
                httpRequestHeaders.Host = null;
            });
            headerRemovers.Add(HttpHeaderNames.Origin, (header, httpRequestHeaders) => {
                httpRequestHeaders.Origin = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.Range, (header, httpRequestHeaders) => {
                httpRequestHeaders.Range = null;
            });
            headerRemovers.Add(HttpHeaderNames.SecWebSocketKey, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketKey = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.SecWebSocketProtocol, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketProtocol = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.SecWebSocketVersion, (header, httpRequestHeaders) => {
                httpRequestHeaders.SecWebSocketVersion = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.TransferEncoding, (header, httpRequestHeaders) => {
                httpRequestHeaders.TransferEncoding = string.Empty;
            });
            headerRemovers.Add(HttpHeaderNames.Upgrade, (header, httpRequestHeaders) => {
                httpRequestHeaders.Upgrade = string.Empty;
            });
        }

        public static HttpRequestHeaders CreateFrom(IEnumerable<HttpHeader> headers)
        {
            var httpRequestHeaders = new HttpRequestHeaders();
            foreach (var header in headers)
            {
                if (headerParsers.TryGetValue(header.Name, out var handler))
                {
                    handler(header, httpRequestHeaders);
                }

                httpRequestHeaders.AddOrUpdate(header);
            }

            return httpRequestHeaders;
        }
    }
}
