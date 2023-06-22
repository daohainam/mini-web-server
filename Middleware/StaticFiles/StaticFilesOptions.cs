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
        public bool UseCompression { get; init; } = true;
        public long MinimumFileSizeToCompress { get; init; } = 1024; // we don't compress too small files
        public long MaximumFileSizeToCompress { get; init; } = long.MaxValue; 
        public string[] FileCompressionMimeTypes { get; init; } = { 
            "text/plain",
            "text/html",
            "text/css",
            "text/javascript"
        };
    }
}
