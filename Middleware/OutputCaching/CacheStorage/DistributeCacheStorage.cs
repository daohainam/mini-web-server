using Microsoft.Extensions.Caching.Distributed;

namespace MiniWebServer.OutputCaching.CacheStorage
{
    public class DistributeCacheStorage : IOutputCacheStorage
    {
        private readonly IDistributedCache cache;

        public DistributeCacheStorage(IDistributedCache cache)
        {
            ArgumentNullException.ThrowIfNull(cache);

            this.cache = cache;
        }
        public OutputCacheStreamInfo GetCachedStream(string cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}
