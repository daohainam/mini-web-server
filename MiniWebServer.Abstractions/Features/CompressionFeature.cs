using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Features;

public class CompressionFeature
{
    public CompressionFeature(int? minimumFileSizeToCompress = null, int? compressionQuality = null, string[]? fileCompressionMimeTypes = null)
    {
        MinimumFileSizeToCompress = minimumFileSizeToCompress ?? 1024;
        CompressionQuality = compressionQuality ?? 5;
        if (CompressionQuality < 0 || CompressionQuality > 11)
        {
            throw new ArgumentOutOfRangeException(nameof(compressionQuality), "compressionQuality must be from 0 (no compression) to 11 (max compression)");
        }
        FileCompressionMimeTypes = fileCompressionMimeTypes ?? [
            "text/plain",
            "text/html",
            "text/css",
            "text/javascript"
        ];
    }
    
    public long MinimumFileSizeToCompress { get; }
    public int CompressionQuality { get; } = 4;
    public string[] FileCompressionMimeTypes { get; }
}
