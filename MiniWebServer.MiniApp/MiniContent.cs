using MiniWebServer.Abstractions;

namespace MiniWebServer.MiniApp
{
    public abstract class MiniContent : IHttpContent
    {
        public abstract Abstractions.Http.HttpHeaders Headers { get; }
        public abstract Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken);
    }
}
