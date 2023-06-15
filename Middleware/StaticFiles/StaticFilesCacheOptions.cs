using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.StaticFiles
{
    public class StaticFilesCacheOptions
    {
        public StaticFilesCacheOptions(long maxAge)
        {
            MaxAge = maxAge;
        }

        public long MaxAge { get; }
    }
}
