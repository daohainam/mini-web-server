namespace MiniWebServer.Authorization
{
    public interface IRouteMatcher
    {
        bool IsMatched(string routeUrl, string url);
    }
}
