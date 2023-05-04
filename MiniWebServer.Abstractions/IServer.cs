namespace MiniWebServer.Abstractions
{
    public interface IServer: IDisposable
    {
        void Start();
        void Stop();
    }
}