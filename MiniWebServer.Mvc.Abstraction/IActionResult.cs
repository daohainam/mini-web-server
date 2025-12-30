namespace MiniWebServer.Mvc.Abstraction;

public interface IActionResult
{
    Task ExecuteResultAsync(ActionResultContext context);
}
