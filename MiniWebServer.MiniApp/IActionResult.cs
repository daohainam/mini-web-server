namespace MiniWebServer.MiniApp
{
    public interface IActionResult
    {
        int StatusCode { get; }
        string ReasonPhrase { get; }
    }
}
