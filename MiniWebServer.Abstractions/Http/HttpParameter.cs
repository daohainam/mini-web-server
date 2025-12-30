namespace MiniWebServer.Abstractions.Http;

public record HttpParameter
{
    private readonly List<string> values;

    public HttpParameter(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ArgumentNullException.ThrowIfNull(value);

        values = [value];
    }

    public HttpParameter(string name, IEnumerable<string> values)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ArgumentNullException.ThrowIfNull(values);

        this.values = new List<string>(values);
    }

    public string Name { get; }
    public IEnumerable<string> Values => values;
    public bool HasValues => Values.Any();
    public string? Value => Values.FirstOrDefault();
    public void AddValue(string value)
    {
        values.Add(value);
    }
    public void AddValue(string value, params string[] strings)
    {
        values.Add(value);
        values.AddRange(strings);
    }
}
