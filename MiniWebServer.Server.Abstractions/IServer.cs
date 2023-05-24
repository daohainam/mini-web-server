namespace MiniWebServer.Server.Abstractions
{
    public interface IServer : IDisposable
    {
        Task Start();
        void Stop();
    }
}