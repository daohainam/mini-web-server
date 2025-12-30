using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http.Header;

public record AuthorizationHeader
{
    public const string SchemeBasic = "Basic";
    public const string SchemeDigest = "Digest";
    public const string SchemeNegotiate = "Negotiate";

    public string Scheme { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
}
