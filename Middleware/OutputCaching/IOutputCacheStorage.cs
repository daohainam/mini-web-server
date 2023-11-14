namespace MiniWebServer.OutputCaching
{
    public interface IOutputCacheStorage
    {
        OutputCacheStreamInfo GetCachedStream(string cacheKey);
    }
}
