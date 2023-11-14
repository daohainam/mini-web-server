namespace MiniWebServer.Session
{
    public class DistributedCacheSessionStoreOptions
    {
        public int LockWaitTimeoutMs { get; set; } = Timeout.Infinite;
    }
}
