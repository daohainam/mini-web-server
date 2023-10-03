using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public sealed record HttpMethod: IEquatable<HttpMethod>
    {
        // https://datatracker.ietf.org/doc/html/rfc9110#name-method-definitions

        private readonly string method;
        public HttpMethod(string method)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public string Method => method;

        public static readonly HttpMethod Get = new("GET");
        public static readonly HttpMethod Head = new("HEAD");
        public static readonly HttpMethod Post = new("POST");
        public static readonly HttpMethod Put = new("PUT");
        public static readonly HttpMethod Delete = new("DELETE");
        public static readonly HttpMethod Connect = new("CONNECT");
        public static readonly HttpMethod Options = new("OPTIONS");
        public static readonly HttpMethod Trace = new("TRACE");
        public static readonly HttpMethod Patch = new("PATCH");

        public override string ToString()
        {
            return method;
        }

        bool IEquatable<HttpMethod>.Equals(HttpMethod? other)
        {
            if (other == null)
                return false;

            if (this.method == other.method)
                return true;
            else
                return false;
        }
    }
}
