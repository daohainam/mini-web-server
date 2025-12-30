namespace MiniWebServer.Mvc.RouteMatchers;

internal class StringEqualsRouteMatcher : IRouteMatcher
{
    // todo: we should accept a regex and match requests based on that
    public bool IsMatched(string requestUrl, string route)
    {
        return string.Equals(requestUrl, route, StringComparison.OrdinalIgnoreCase);
    }
}
