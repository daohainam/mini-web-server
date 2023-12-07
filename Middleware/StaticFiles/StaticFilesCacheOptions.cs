namespace MiniWebServer.StaticFiles
{
    public class StaticFilesCacheOptions(long maxAge)
    {
        public long MaxAge { get; } = maxAge;
    }
}
