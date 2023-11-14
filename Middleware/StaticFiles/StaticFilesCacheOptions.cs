namespace MiniWebServer.StaticFiles
{
    public class StaticFilesCacheOptions
    {
        public StaticFilesCacheOptions(long maxAge)
        {
            MaxAge = maxAge;
        }

        public long MaxAge { get; }
    }
}
