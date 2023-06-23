using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.StaticFiles
{
    public class StaticFilesOptions
    {
        public StaticFilesOptions(string? root = null, string[]? defaultDocuments = null, StaticFilesCacheOptions? cacheOptions = null, bool? useCompression = null, int? minimumFileSizeToCompress = null, int? compressionQuality = null, string[]? fileCompressionMimeTypes = null)
        {
            Root = root ?? "wwwroot";
            DefaultDocuments = defaultDocuments ?? new string[] { "index.htm", "index.html" };
            CacheOptions = cacheOptions ?? new StaticFilesCacheOptions(0);
            UseCompression = useCompression ?? true;
            MinimumFileSizeToCompress = minimumFileSizeToCompress ?? 1024;
            CompressionQuality = compressionQuality ?? 5;
            if (CompressionQuality < 0 || CompressionQuality > 11)
            {
                throw new ArgumentOutOfRangeException(nameof(compressionQuality), "compressionQuality must be from 0 (no compression) to 11 (max compression)");
            }
            FileCompressionMimeTypes = fileCompressionMimeTypes ?? new string[] {
                "text/plain",
                "text/html",
                "text/css",
                "text/javascript"
            };
        }

        public string Root { get; }
        public string[] DefaultDocuments { get; }
        public StaticFilesCacheOptions CacheOptions { get; }
        public bool UseCompression { get; }
        public long MinimumFileSizeToCompress { get; }
        public int CompressionQuality { get; } = 5;
        public string[] FileCompressionMimeTypes { get; }
    }
}
