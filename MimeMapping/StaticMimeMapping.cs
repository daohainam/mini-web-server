using MiniWebServer.Abstractions;

namespace MimeMapping
{
    public class StaticMimeMapping : IMimeTypeMapping
    {
        private static StaticMimeMapping? instance = null;
        private static readonly object instanceLock = new();
        private readonly Dictionary<string, string> mimeTypes = new();

        private StaticMimeMapping() { } // a private constructor prevents creating new instances from outside

        public string GetMimeMapping(string fileExt)
        {
            if (mimeTypes.TryGetValue(fileExt, out var mimeType))
            {
                return mimeType;
            }

            return "application/octet-stream"; // default binary data
        }

        // this function demonstrates Sleleton pattern
        public static StaticMimeMapping GetInstance() { 
            lock (instanceLock)
            {
                if (instance == null)
                {
                    // for a full list (extremely long), refer https://www.iana.org/assignments/media-types/media-types.xhtml
                    instance = new StaticMimeMapping();
                    instance.mimeTypes.Add(".txt", "text/plain");
                    instance.mimeTypes.Add(".htm", "text/html");
                    instance.mimeTypes.Add(".html", "text/html");
                    instance.mimeTypes.Add(".jpg", "image/jpeg");
                    instance.mimeTypes.Add(".bmp", "image/bmp");
                    instance.mimeTypes.Add(".png", "image/png");
                }
            }
            return instance;
        }
    }
}