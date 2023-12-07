using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using System.Text;
using System.Text.Json;

namespace MiniWebServer.Session
{
    public class DistributedCacheSession : ISession
    {
        private const string KeyPrefix = "_#M.S#_";

        private readonly Dictionary<string, byte[]> localStore = [];
        private SpinLock spinLock = new(); // do not make it readonly, or .NET will create a new shadow instance to keep it immutable
        private readonly int waitTimeoutMs = 0;

        public DistributedCacheSession(string id, IDistributedCache cache, ILoggerFactory? loggerFactory = default, int waitTimeoutMs = -1)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.waitTimeoutMs = waitTimeoutMs;

            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger(nameof(DistributedCacheSession));
            }
            else
            {
                logger = NullLogger<DistributedCacheSession>.Instance;
            }

            // by default we will wait forever
            if (this.waitTimeoutMs < -1)
            {
                this.waitTimeoutMs = -1;
            }
        }

        public string Id { get; }

        private readonly IDistributedCache cache;
        private readonly ILogger logger;

        public bool IsAvaiable => true;

        public byte[]? Get(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                bool keyFound = localStore.TryGetValue(key, out var value);
                spinLock.Exit();

                return keyFound ? value : null;
            }
            else
            {
                logger.LogWarning("Get: Lock could not be taken");
            }

            return null;
        }

        public async Task<bool> LoadAsync()
        {
            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                try
                {
                    localStore.Clear();

                    string key = KeyPrefix + Id;
                    var jsonString = await cache.GetStringAsync(key);
                    logger.LogDebug("Loaded session {id}: {value}", key, jsonString);

                    if (jsonString != null)
                    {
                        var model = JsonSerializer.Deserialize<SessionJsonModel>(jsonString);

                        if (model != null)
                        {
                            foreach (var item in model)
                            {
                                byte[] value = Encoding.UTF8.GetBytes(item.Value);
                                localStore[item.Key] = value;
                            }
                        }
                        else
                        {
                            logger.LogWarning("Could not deserialize session JSON");
                        }
                    }
                }
                finally { spinLock.Exit(); }

                return true;
            }
            else
            {
                logger.LogWarning("LoadAsync: Lock could not be taken");

                return false;
            }
        }

        public bool Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                var b = localStore.Remove(key);
                spinLock.Exit();

                return b;
            }
            else
            {
                logger.LogWarning("Remove: Lock could not be taken");
            }

            return false;
        }

        public async Task<bool> SaveAsync()
        {
            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                try
                {
                    string key = KeyPrefix + Id;
                    var model = new SessionJsonModel();

                    foreach (var item in localStore)
                    {
                        string jsonValue = Encoding.UTF8.GetString(item.Value);
                        model[item.Key] = jsonValue;
                    }

                    var jsonString = JsonSerializer.Serialize(model);
                    logger.LogDebug("Saving session {id}: {value}", key, jsonString);
                    await cache.SetStringAsync(key, jsonString);
                }
                finally { spinLock.Exit(); }

                return true;
            }
            else
            {
                logger.LogWarning("LoadAsync: Lock could not be taken");

                return false;
            }
        }

        public byte[]? Set(string key, byte[] value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value); // we don't allow null values

            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                localStore[key] = value;
                spinLock.Exit();

                return value;
            }
            else
            {
                logger.LogWarning("Set: Lock could not be taken");
            }

            return null; // Set failed
        }

        public bool Clear()
        {
            bool lockTaken = false;
            spinLock.TryEnter(waitTimeoutMs, ref lockTaken);

            if (lockTaken)
            {
                localStore.Clear();
                spinLock.Exit();

                return true;
            }
            else
            {
                logger.LogWarning("Remove: Lock could not be taken");
            }

            return false;
        }

        public string? GetString(string key)
        {
            var bytes = Get(key);
            if (bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public string? SetString(string key, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            if (Set(key, bytes) != null)
            {
                return value;
            }

            return null;
        }
    }
}
