using Microsoft.Extensions.Caching.Distributed;
using MiniWebServer.Server.Abstractions;
using System.Text;

namespace MiniWebServer.Server.MimeType
{

    /// <summary>
    /// since mime type 'database' rarely (or never) changes, we created this class only to demonstrate how to use a cache 
    /// this class uses Decorator pattern
    /// </summary>
    public class CachableMimeTypeMapping(IMimeTypeMapping parent, IDistributedCache cache) : IMimeTypeMapping
    {
        private readonly IMimeTypeMapping parent = parent ?? throw new ArgumentNullException(nameof(parent));
        private readonly IDistributedCache cache = cache ?? throw new ArgumentNullException(nameof(cache));

        public string GetMimeMapping(string fileExt)
        {
            // this class uses 'Read-aside caching strategy'
            var key = "mime#" + fileExt;
            var item = cache.GetString(key);

            if (item == null)
            {
                item = parent.GetMimeMapping(fileExt);
                cache.SetString(key, item, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365), // cache it in 1 year :D
                });
            }

            return item;
        }
    }
}
