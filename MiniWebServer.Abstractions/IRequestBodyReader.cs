namespace MiniWebServer.Abstractions
{
    public interface IRequestBodyReader
    {
        Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default);
    }
}