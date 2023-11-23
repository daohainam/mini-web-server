using MiniWebServer.MiniApp;

namespace MiniWebServer.OutputCaching
{
    public interface IOutputCacheKeyGenerator
    {
        string GenerateCacheKey(IMiniAppRequestContext context);
    }
}
