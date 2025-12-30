namespace MiniWebServer.Abstractions.Http;

public class HttpCookies : Dictionary<string, HttpCookie>
{
    public HttpCookies()
    {
    }

    public HttpCookies(IDictionary<string, HttpCookie> cookies) : base(cookies)
    {
    }

    public HttpCookies(IEnumerable<HttpCookie> cookies)
    {
        ArgumentNullException.ThrowIfNull(cookies);

        foreach (var cookie in cookies)
        {
            if (!ContainsKey(cookie.Name))
                Add(cookie.Name, cookie);
            else
                this[cookie.Name] = cookie;
        }
    }

    public bool Add(HttpCookie cookie)
    {
        return TryAdd(cookie.Name, cookie);
    }
    public bool Add(HttpCookies cookies)
    {
        foreach (var cookie in cookies)
        {
            var b = TryAdd(cookie.Key, cookie.Value);

            if (!b)
                return b;
        }

        return true;
    }
}
