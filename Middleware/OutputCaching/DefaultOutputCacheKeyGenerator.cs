using MiniWebServer.MiniApp;

namespace MiniWebServer.OutputCaching;

public class DefaultOutputCacheKeyGenerator : IOutputCacheKeyGenerator
{
    public string GenerateCacheKey(IMiniAppRequestContext context)
    {
        var key = $"{context.Request.Host}##{context.Request.Port}##{context.Request.Method}##{context.Request.Url}##{context.Request.Hash}##{context.Request.QueryString}";

        return key;
    }
}
