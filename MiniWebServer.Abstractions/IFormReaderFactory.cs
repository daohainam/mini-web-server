namespace MiniWebServer.Abstractions;

public interface IFormReaderFactory
{
    IFormReader? CreateFormReader(string contentType, long contentLength);
}
