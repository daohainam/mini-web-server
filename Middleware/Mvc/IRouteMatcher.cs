namespace MiniWebServer.Mvc
{
    public interface IRouteMatcher
    {
        bool IsMatched(string requestUrl, string route);
    }
}
