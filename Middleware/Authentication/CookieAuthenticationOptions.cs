namespace MiniWebServer.Authentication
{
    public class CookieAuthenticationOptions
    {
        public const string DefaultCookieName = ".miniWeb.UID";

        public CookieAuthenticationOptions(string? cookieName = null)
        {
            CookieName = cookieName ?? DefaultCookieName;
            if (CookieName.Length == 0)
            {
                throw new ArgumentException(nameof(cookieName) + " cannot be empty");
            }

        }
        public string CookieName { get; }
    }
}
