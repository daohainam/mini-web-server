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
            throw new NotImplementedException();
        }
    }
}
