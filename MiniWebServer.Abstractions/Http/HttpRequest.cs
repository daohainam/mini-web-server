using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions.Http.Form;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Text;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpRequest : IHttpRequest
    {
        private static readonly IRequestForm EmptyForm = new RequestForm();

        public HttpRequest(ulong requestId,
            HttpMethod method,
            string host,
            int port,
            string url,
            HttpRequestHeaders headers,
            string queryString,
            string hash,
            HttpParameters queryParameters,
            string[] segments,
            HttpCookies cookies,
            Pipe bodyPipeline,
            long contentLength,
            string contentType,
            bool isHttps,
            IPAddress? remoteAddress,
            int remotePort,
            HttpVersions httpVersion
            )
        {
            ArgumentNullException.ThrowIfNull(queryParameters);

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

            RequestId = requestId;
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            QueryString = queryString ?? string.Empty;
            Hash = hash ?? string.Empty;
            QueryParameters = queryParameters ?? throw new ArgumentNullException(nameof(queryParameters));
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
            Cookies = cookies ?? throw new ArgumentNullException(nameof(cookies));
            BodyPipeline = bodyPipeline;
            ContentLength = contentLength;
            ContentType = contentType;
            IsHttps = isHttps;
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
            HttpVersion = httpVersion;

            BodyManager = new RequestBodyManager(BodyPipeline.Reader);
        }

        public Pipe BodyPipeline { get; }
        public HttpCookies Cookies { get; }
        public long ContentLength { get; }
        public string ContentType { get; }
        public bool IsHttps { get; }
        public IPAddress? RemoteAddress { get; }
        public int RemotePort { get; }
        public string Hash { get; }
        public ulong RequestId { get; }
        public HttpMethod Method { get; }
        public string Host { get; }
        public int Port { get; }
        public HttpRequestHeaders Headers { get; }
        public HttpParameters QueryParameters { get; }
        public string[] Segments { get; }
        public string QueryString { get; }
        public string Url { get; }
        public HttpVersions HttpVersion { get; }

        private IRequestForm? form; // a cache object for lazy loading 

        public bool KeepAliveRequested
        {
            get
            {
                return !"close".Equals(Headers.Connection, StringComparison.OrdinalIgnoreCase); // it is keep-alive by default
            }
        }

        public IRequestBodyManager BodyManager { get; }

        public async Task<IRequestForm> ReadFormAsync(ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
        {
            if (form != null)
                return form;
            else
            {
                if (ContentType == null)
                    return EmptyForm;

                var reader = BodyManager.GetReader();

                if (reader == null)
                {
                    return EmptyForm;
                }

                var formReaderFactory = new DefaultFormReaderFactory(loggerFactory);

                var formReader = formReaderFactory.CreateFormReader(ContentType, ContentLength) ?? throw new InvalidHttpStreamException("Not supported content type");
                var readform = await formReader.ReadAsync(reader, cancellationToken);
                form = readform ?? throw new InvalidHttpStreamException("Error reading form data");

                return form;
            }
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            var contentLength = ContentLength;
            if (contentLength > 0)
            {
                var reader = BodyManager.GetReader() ?? throw new InvalidOperationException("Body reader cannot be null");
                var encoding = Encoding.UTF8; // todo: we should take the right encoding from Content-Type
                var sb = new StringBuilder();

                ReadResult readResult = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                long bytesRead = 0;
                while (bytesRead < contentLength)
                {
                    long maxBytesToRead = contentLength - bytesRead;
                    if (buffer.Length >= maxBytesToRead)
                    {
                        sb.Append(encoding.GetString(buffer.Slice(0, maxBytesToRead))); // what will happen if a multi-byte character is partly sent?

                        reader.AdvanceTo(buffer.GetPosition(maxBytesToRead));
                        break;
                    }
                    else if (buffer.Length > 0)
                    {
                        sb.Append(encoding.GetString(buffer));
                        reader.AdvanceTo(buffer.GetPosition(buffer.Length));

                        bytesRead += buffer.Length;
                    }

                    readResult = await reader.ReadAsync(cancellationToken);
                    buffer = readResult.Buffer;
                }

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
