using MiniWebServer.Server.Abstractions;

namespace MiniWebServer.Server.MimeType;

// this class uses Decorator pattern
public class MimeTypeMappingContainer(IEnumerable<IMimeTypeMapping> mappings) : IMimeTypeMapping
{
    private readonly IEnumerable<IMimeTypeMapping> mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));

    public string GetMimeMapping(string fileExt)
    {
        foreach (var mapping in mappings)
        {
            var mimeType = mapping.GetMimeMapping(fileExt);
            if (mimeType != null)
            {
                return mimeType;
            }
        }

        return "application/octet-stream";
    }
}
