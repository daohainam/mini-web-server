namespace MiniWebServer.Session
{
    public class SessionOptions(string? sessionIdKey = default)
    {
        public const string DefaultSessionIdKey = ".miniWeb.SID";

        public string SessionIdKey { get; } = sessionIdKey ?? DefaultSessionIdKey;
    }
}
