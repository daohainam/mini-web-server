using MiniWebServer.Abstractions;

namespace MvcMiddlewareTests
{
    internal class RequestBodyReader : IRequestBodyReader
    {
        private readonly string body;

        public RequestBodyReader(string body)
        {
            this.body = body ?? string.Empty;
        }

        public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(body);
        }
    }
}
