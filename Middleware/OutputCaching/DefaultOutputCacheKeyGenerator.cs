using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.OutputCaching
{
    public class DefaultOutputCacheKeyGenerator : IOutputCacheKeyGenerator
    {
        public string GenerateCacheKey(IMiniAppContext context)
        {
            var key = $"{context.Request.Host}##{context.Request.Port}##{context.Request.Method}##{context.Request.Url}##{context.Request.Hash}##{context.Request.QueryString}";

            return key;
        }
    }
}
