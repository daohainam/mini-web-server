namespace MiniWebServer.Abstractions.Http
{
    public record HttpHeader
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

        public bool ValueEquals(string value)
        {
            return Value.Any(x => x.Equals(value));
        }
    }
}
