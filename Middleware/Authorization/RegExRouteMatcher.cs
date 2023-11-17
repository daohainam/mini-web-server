namespace MiniWebServer.Authorization
{
    public class RegExRouteMatcher : IRouteMatcher
    {
        public bool IsMatched(string routeUrl, string url)
        {
            // we will replace with RegEx later

            return string.Compare(routeUrl, url, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
