using System.IO.Pipelines;

namespace MiniWebServer.Abstractions;

public interface IFormReader
{
    Task<IRequestForm?> ReadAsync(PipeReader pipeReader, CancellationToken cancellationToken = default);
}
