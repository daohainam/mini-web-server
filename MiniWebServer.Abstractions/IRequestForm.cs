using Microsoft.Extensions.Primitives;

namespace MiniWebServer.Abstractions;

public interface IRequestForm : IEnumerable<KeyValuePair<string, StringValues>>
{
    public int Count { get; }
    ICollection<string> Keys { get; }
    StringValues this[string key] { get; }
    bool TryGetValue(string key, out StringValues values);
}
