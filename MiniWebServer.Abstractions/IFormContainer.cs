using Microsoft.Extensions.Logging;

namespace MiniWebServer.Abstractions;

public interface IFormContainer
{
    Task<IRequestForm> ReadFormAsync(ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default);
}
