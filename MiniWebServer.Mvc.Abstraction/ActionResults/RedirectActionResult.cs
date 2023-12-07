namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class RedirectActionResult(string url, bool permanent) : IActionResult
    {
        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = permanent ? Abstractions.HttpResponseCodes.PermanentRedirect : Abstractions.HttpResponseCodes.TemporaryRedirect;
            context.Response.Headers.Location = url;
            context.Response.Content = MiniApp.Content.StringContent.Empty;

            return Task.CompletedTask;
        }
    }
}
