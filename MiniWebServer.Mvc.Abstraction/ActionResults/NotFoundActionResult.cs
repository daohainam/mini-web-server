namespace MiniWebServer.Mvc.Abstraction.ActionResults;

public class NotFoundActionResult(object? value) : IActionResult
{
    public Task ExecuteResultAsync(ActionResultContext context)
    {
        context.Response.StatusCode = Abstractions.HttpResponseCodes.NotFound;

        if (value != null)
        {
            var valueString = value.ToString();
            context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);
        }
        else
        {
            context.Response.Content = MiniApp.Content.StringContent.Empty;
        }

        return Task.CompletedTask;
    }
}
