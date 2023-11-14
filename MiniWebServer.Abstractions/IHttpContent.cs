using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IHttpContent
    {
        HttpHeaders Headers { get; }
        Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken);
    }
}
