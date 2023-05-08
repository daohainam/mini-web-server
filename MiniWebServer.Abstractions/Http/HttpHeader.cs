using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpHeader: IEquatable<HttpHeader>
    {
        public HttpHeader(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = new List<string>() { value } ?? throw new ArgumentNullException(nameof(value));
        }
        public HttpHeader(string name, IEnumerable<string> value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = new List<string>(value) ?? throw new ArgumentNullException(nameof(value));
        }
        public string Name { get; }
        public IEnumerable<string> Value { get; }

        public bool Equals(HttpHeader? other)
        {
            if (other == null) return false;

            return Name.Equals(other.Name) && Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as HttpHeader);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Value.GetHashCode();
        }

        public bool ValueEquals(string value)
        {
            return Value.Any(x => x.Equals(value));
        }
    }
}
