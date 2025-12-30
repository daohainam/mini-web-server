using MiniWebServer.Server.Abstractions;

namespace MiniWebServer.MiniWebServer.MimeMapping;

public class StaticMimeMapping : IMimeTypeMapping
{
    private static readonly StaticMimeMapping instance = new()
    {
        // for a full list (extremely long), refer https://www.iana.org/assignments/media-types/media-types.xhtml
        mimeTypes =
        {
            {".txt", "text/plain"},
            {".htm", "text/html"},
            {".css", "text/css"},
            {".js", "application/x-javascript"},
            {".html", "text/html"},
            {".jpg", "image/jpeg"},
            {".bmp", "image/bmp"},
            {".png", "image/png"},
            {".xml", "text/xml"}
        }
    };

    private readonly Dictionary<string, string> mimeTypes = [];

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static StaticMimeMapping()
    {
    }
    private StaticMimeMapping() { } // a private constructor prevents creating new instances from outside

    public string GetMimeMapping(string fileExt)
    {
        if (mimeTypes.TryGetValue(fileExt, out var mimeType))
        {
            return mimeType;
        }

        return "application/octet-stream"; // default binary data
    }

    // this function demonstrates singleton pattern
    public static StaticMimeMapping Instance => instance;
}
