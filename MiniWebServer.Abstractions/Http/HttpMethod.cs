using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpMethod
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

        public bool Equals(HttpMethod? other)
        {
            return other is not null && ReferenceEquals(this, other);
        }

        public override bool Equals(object? other)
        {
            return Equals(other as HttpMethod);
        }

        public static bool operator ==(HttpMethod? m1, HttpMethod? m2)
        {
            return m1 is null || m2 is null ? ReferenceEquals(m1, m2) : m1.Equals(m2);
        }

        public static bool operator !=(HttpMethod? m1, HttpMethod? m2)
        {
            return !(m1 == m2);
        }

        public override int GetHashCode()
        {
            return method.GetHashCode();
        }
    }
}
