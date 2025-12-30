using System.Security.Principal;

namespace MiniWebServer.Authentication;

internal class DefaultPrincipalKeyGenerator : IPrincipalKeyGenerator
{
    //private static readonly Random random = new();
    //private readonly int length;

    public DefaultPrincipalKeyGenerator()
    {
        //if (length < 16)
        //{
        //    throw new ArgumentOutOfRangeException("length must be >= 16");
        //}
        //this.length = length;
    }

    public string? GeneratePrincipalKey(IPrincipal principal)
    {
        return principal.Identity?.Name;

        //return GenerateRandomString(length);
    }

    //private static string GenerateRandomString(int length)
    //{
    //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    //    return new string(Enumerable.Repeat(chars, length)
    //        .Select(s => s[random.Next(s.Length)]).ToArray());
    //}
}
