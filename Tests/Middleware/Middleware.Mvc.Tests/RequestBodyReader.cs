using MiniWebServer.Abstractions;

namespace MvcMiddlewareTests;

internal class RequestBodyReader(string body) : IRequestBodyReader
{
    private readonly string body = body ?? string.Empty;

    public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(body);
    }
}
