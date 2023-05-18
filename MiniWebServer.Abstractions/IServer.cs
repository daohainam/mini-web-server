namespace MiniWebServer.Abstractions
{
    public interface IServer: IDisposable
    {
        Task Start();
        void Stop();
    }
}