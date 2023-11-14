namespace MiniWebServer.MiniApp
{
    public interface IMiddlewareFactory
    {
        IMiddleware Create(Type middlewareType);
    }
}
