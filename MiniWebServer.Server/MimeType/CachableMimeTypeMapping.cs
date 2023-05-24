using Microsoft.Extensions.Caching.Distributed;
using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MimeType
{

    /// <summary>
    /// since mime type 'database' rarely (or never) changes, we created this class only to demonstrate how to use a cache 
    /// this class uses Decorator pattern
    /// </summary>
    public class CachableMimeTypeMapping : IMimeTypeMapping
    {
        private readonly IMimeTypeMapping parent;
        private readonly IDistributedCache cache;

        public CachableMimeTypeMapping(IMimeTypeMapping parent, IDistributedCache cache)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public string GetMimeMapping(string fileExt)
        {
            // this class uses 'Read-aside caching strategy'
            var key = "mime#" + fileExt;
            var item = cache.GetString(key);

            if (item == null)
            {
                item = parent.GetMimeMapping(fileExt);
                cache.SetString(key, item, new DistributedCacheEntryOptions() { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365), // cache it in 1 year :D
                });
            }

            return item;
        }
    }
}
