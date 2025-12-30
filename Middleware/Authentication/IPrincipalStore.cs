using System.Security.Claims;

namespace MiniWebServer.Authentication;

public interface IPrincipalStore
{
    ClaimsPrincipal? GetPrincipal(string key);
    bool RemovePrincipal(string key);
    bool SetPrincipal(string key, ClaimsPrincipal principal);
}
