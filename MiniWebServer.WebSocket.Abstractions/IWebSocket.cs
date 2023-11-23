namespace MiniWebServer.WebSocket.Abstractions
{
    public interface IWebSocket
    {
        Task SendAsync(Memory<byte> bytes, CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(Memory<byte> bytes, CancellationToken cancellationToken);
        Task CloseAsync(CancellationToken cancellationToken);
    }
}
