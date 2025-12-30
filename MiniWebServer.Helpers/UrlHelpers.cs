using System.Net;

namespace MiniWebServer.Helpers;

public class UrlHelpers
{
    // I will use WebUtility class for now 

    public static string UrlDecode(string encodedString)
    {
        return WebUtility.UrlDecode(encodedString);
    }

    public static string UrlDecode(byte[] encodedBytes, int offset, int count)
    {
        return UrlDecode(encodedBytes, offset, count);
    }
}
