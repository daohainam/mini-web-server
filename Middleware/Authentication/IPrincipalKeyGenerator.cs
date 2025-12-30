using System.Security.Principal;

namespace MiniWebServer.Authentication;

public interface IPrincipalKeyGenerator
{
    string? GeneratePrincipalKey(IPrincipal principal);
}
