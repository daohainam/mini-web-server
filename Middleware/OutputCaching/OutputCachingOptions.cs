namespace MiniWebServer.OutputCaching;

public class OutputCachingOptions
{
    public OutputCachingOptions()
    {
        Policies = [];
    }

    public ICollection<IOutputCachePolicy> Policies { get; }
    public IOutputCacheStorage? OutputCacheStorage { get; set; }
}
