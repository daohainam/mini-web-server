using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;

namespace MiniWebServer.Session
{
    public class DistributedCacheSessionStore : ISessionStore
    {
        private readonly IDistributedCache cache;
        private readonly ILoggerFactory? loggerFactory;
        private readonly ILogger logger;
        private readonly DistributedCacheSessionStoreOptions options;

        public DistributedCacheSessionStore(IDistributedCache? cache, ILoggerFactory? loggerFactory = default, DistributedCacheSessionStoreOptions? options = default)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.loggerFactory = loggerFactory;

            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger(nameof(DistributedCacheSession));
            }
            else
            {
                logger = NullLogger<DistributedCacheSession>.Instance;
            }

            this.options = options ?? new();
        }

        public ISession Create(string sessionId)
        {
            return new DistributedCacheSession(sessionId, cache, loggerFactory, options.LockWaitTimeoutMs);
        }
    }
}
