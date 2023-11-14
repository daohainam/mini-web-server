namespace MiniWebServer.HstsMiddleware
{
    public class HstsOptions
    {
        public int MaxAge { get; set; } = 63072000; // two years is recommended: https://hstspreload.org/
        public bool IncludeSubDomains { get; set; } = true;
        public bool Preload { get; set; } = true;
    }
}
