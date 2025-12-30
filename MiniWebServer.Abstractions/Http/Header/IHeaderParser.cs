namespace MiniWebServer.Abstractions.Http.Header;

public interface IHeaderParser<T>
{
    bool TryParse(string? value, out T? v);
}
