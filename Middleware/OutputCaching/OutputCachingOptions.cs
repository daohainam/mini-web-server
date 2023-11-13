using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
