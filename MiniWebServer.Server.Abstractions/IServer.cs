using Microsoft.Extensions.Hosting;

namespace MiniWebServer.Server.Abstractions;

public interface IServer : IHostedService, IDisposable
{
    Task Start(CancellationToken? cancellationToken = default);
    void Stop(CancellationToken? cancellationToken = default);
}
