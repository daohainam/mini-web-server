namespace MiniWebServer.Abstractions.Http;

public class EmptyContent : IHttpContent
{
    private readonly HttpHeaders headers;

    public EmptyContent()
    {
        headers = [];
    }

    public HttpHeaders Headers => headers;

    public async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        return await Task.FromResult(0);
    }

    public static EmptyContent Instance => new();
}
