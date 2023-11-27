namespace MiniWebServer.WebSocket.Abstractions
{
    public interface IWebSocket
    {
        Task SendAsync(Memory<byte> bytes, CancellationToken cancellationToken = default);
        Task<WebSocketReceiveResult> ReceiveAsync(Memory<byte> bytes, CancellationToken cancellationToken = default);
        Task CloseAsync(CancellationToken cancellationToken = default);
    }
}
