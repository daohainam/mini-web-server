using System.Collections.Concurrent;
using System.Security.Claims;

namespace MiniWebServer.Authentication;

public class MemoryPrincipalStore : IPrincipalStore
{
    private static readonly ConcurrentDictionary<string, ClaimsPrincipal> principals = new();

    public ClaimsPrincipal? GetPrincipal(string key)
    {
        if (principals.TryGetValue(key, out ClaimsPrincipal? principal))
        {
            return principal;
        }

        return null;
    }

    public bool RemovePrincipal(string key)
    {
        return principals.Remove(key, out ClaimsPrincipal? _);
    }

    public bool SetPrincipal(string key, ClaimsPrincipal principal)
    {
        return principals.TryAdd(key, principal);
    }
}
