namespace MiniWebServer.Authorization;

public static class RouteDictionaryExtensions
{
    public static void Add(this IDictionary<string, string[]> dictionary, string key, string value, params string[]? values)
    {
        List<string> strings =
        [
            value
        ];

        if (values != null)
        {
            foreach (string v in values)
            {
                strings.Add(v);
            }
        }
        dictionary[key] = [.. strings];
    }
}
