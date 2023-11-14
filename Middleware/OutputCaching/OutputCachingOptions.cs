namespace MiniWebServer.OutputCaching
{
    public class OutputCachingOptions
    {
        public OutputCachingOptions()
        {
            Policies = new List<IOutputCachePolicy>();
        }

        public ICollection<IOutputCachePolicy> Policies { get; }
        public IOutputCacheStorage? OutputCacheStorage { get; set; }
    }
}
