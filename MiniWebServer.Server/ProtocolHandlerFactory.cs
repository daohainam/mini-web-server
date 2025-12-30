using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Parsers;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using MiniWebServer.Server.Cookie;
using MiniWebServer.Server.ProtocolHandlers.Http11;
using MiniWebServer.Server.ProtocolHandlers.Http2;

namespace MiniWebServer.Server;

public class ProtocolHandlerFactory(ILoggerFactory loggerFactory, IServiceProvider services) : IProtocolHandlerFactory
{
    private readonly ILoggerFactory loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    private readonly IServiceProvider services = services ?? throw new ArgumentNullException(nameof(services));

    public IProtocolHandler Create(HttpVersions httpVersion, ProtocolHandlerConfiguration config, ProtocolHandlerContext protocolHandlerContext)
    {
        if (httpVersion == HttpVersions.Http11)
        {
            // in reality we often use default parsers

            return new Http11ProtocolHandler(config, loggerFactory,
                services.GetService<IHttpComponentParser>() ?? new ByteSequenceHttpParser(loggerFactory),
                services.GetService<ICookieValueParser>() ?? new DefaultCookieParser(),
                protocolHandlerContext
                );
        }
        else if (httpVersion == HttpVersions.Http20)
        {
            return new Http2ProtocolHandler(loggerFactory,
                services.GetService<IHttpComponentParser>() ?? new ByteSequenceHttpParser(loggerFactory),
                services.GetService<ICookieValueParser>() ?? new DefaultCookieParser(),
                protocolHandlerContext
                );
        }

        throw new ArgumentOutOfRangeException(nameof(httpVersion), $"Unknown protocol version: {httpVersion}");
    }
}
