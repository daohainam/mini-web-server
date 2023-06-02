using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public sealed class HttpCookie
    {
        public HttpCookie(string name, string value, 
            DateTime? expires = default,
            bool? secure = default,
            bool? httpOnly = default,
            long? maxAge = default,
            string? path = default
            )
        {
            EnsureValidName(name);

            Name = name;
            Value = value ?? throw new ArgumentNullException(nameof(value));

            Expires = expires;
            Secure = secure;
            HttpOnly = httpOnly;
            MaxAge = maxAge;
            Path = path;
        }

        public string Name { get; }
        public string Value { get; }
        public DateTime? Expires { get; }
        public bool? Secure { get; }
        public bool? HttpOnly { get; }
        public long? MaxAge { get; }
        public string? Path { get; }

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
    }
}
