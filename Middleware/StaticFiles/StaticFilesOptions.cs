using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.StaticFiles
{
    public class StaticFilesOptions
    {
        public string Root { get; init; } = "wwwroot";
        public string[] DefaultDocuments { get; init; } = new string[] { "index.htm", "index.html" };
        public StaticFilesCacheOptions CacheOptions { get; init; } = new StaticFilesCacheOptions(0);
    }
}
