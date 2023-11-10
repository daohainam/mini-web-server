using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.OutputCaching
{
    public interface IOutputCacheKeyGenerator
    {
        string GenerateCacheKey(IMiniAppContext context);
    }
}
