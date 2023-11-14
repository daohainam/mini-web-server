using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.MiniApp.Content
{
    public class ByteArrayContent : MiniContent
    {
        private readonly byte[] content;
        private readonly HttpHeaders headers;

        public ByteArrayContent(byte[] content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            headers = new() {
                { "Content-Length", content.Length.ToString() }
            };
        }

        public override HttpHeaders Headers => headers;

        public override async Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            stream.Write(content);

            return await Task.FromResult(content.Length);
        }
    }
}
