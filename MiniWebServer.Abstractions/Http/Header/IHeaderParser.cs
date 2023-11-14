namespace MiniWebServer.Abstractions.Http.Header
{
    public interface IHeaderParser
    {
        object? Parse(string? value);
    }
}
