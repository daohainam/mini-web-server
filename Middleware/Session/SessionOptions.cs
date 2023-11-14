namespace MiniWebServer.Session
{
    public class SessionOptions
    {
        public const string DefaultSessionIdKey = ".miniWeb.SID";

        public SessionOptions(string? sessionIdKey = default)
        {
            SessionIdKey = sessionIdKey ?? DefaultSessionIdKey;
        }

        public string SessionIdKey { get; } = DefaultSessionIdKey;
    }
}
