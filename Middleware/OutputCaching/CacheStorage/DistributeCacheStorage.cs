using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.OutputCaching.CacheStorage
{
    public class DistributeCacheStorage : IOutputCacheStorage
    {
        private readonly IDistributedCache cache;

        public DistributeCacheStorage(IDistributedCache cache) { 
            ArgumentNullException.ThrowIfNull(cache);

            this.cache = cache;
        }
        public OutputCacheStreamInfo GetCachedStream(string cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}
