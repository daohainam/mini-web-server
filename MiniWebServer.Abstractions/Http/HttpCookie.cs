using System.Globalization;
using System.Text;

namespace MiniWebServer.Abstractions.Http;

public record HttpCookie
{
    private static CultureInfo enUS = new("en-US");
    public HttpCookie(string name, string value,
        string? domain = default,
        DateTimeOffset? expires = default,
        bool? httpOnly = default,
        long? maxAge = default,
        string? path = default,
        SameSitePolicies? sameSite = default,
        bool? secure = default
        )
    {
        EnsureValidName(name);

        Name = name;
        Value = value ?? throw new ArgumentNullException(nameof(value));

        Domain = domain;
        Expires = expires;
        Secure = secure;
        HttpOnly = httpOnly;
        MaxAge = maxAge;
        Path = path;
        SameSite = sameSite;
    }

    public string Name { get; }
    public string Value { get; }
    public string? Domain { get; }
    public DateTimeOffset? Expires { get; }
    public bool? Secure { get; }
    public bool? HttpOnly { get; }
    public long? MaxAge { get; }
    public string? Path { get; }
    public SameSitePolicies? SameSite { get; }

    public enum SameSitePolicies
    {
        None,
        Lax,
        Strict
    }

    private static void EnsureValidName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        foreach (var c in name)
        {
            if (char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
                continue;

            throw new InvalidOperationException();
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Name);
        sb.Append('=');
        sb.Append(Value);

        if (Domain != null)
        {
            sb.Append("; Domain=");
            sb.Append(Domain);
        }
        if (Expires.HasValue)
        {
            sb.Append("; Expires=");
            sb.Append(Expires.Value.UtcDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss G\\MT", enUS));
        }
        if (HttpOnly.HasValue)
        {
            sb.Append("; HttpOnly");
        }
        if (MaxAge.HasValue)
        {
            sb.Append("; Max-Age=");
            sb.Append(MaxAge.Value);
        }
        if (Path != null)
        {
            sb.Append("; Path=");
            sb.Append(Path);
        }
        if (SameSite != null)
        {
            sb.Append("; SameSite=");
            sb.Append(SameSite.Value.ToString());
        }
        if (Secure.HasValue)
        {
            sb.Append("; Secure");
        }

        return sb.ToString();
    }
}
