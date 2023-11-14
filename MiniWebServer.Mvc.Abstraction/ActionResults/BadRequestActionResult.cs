namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class BadRequestActionResult : IActionResult
    {
        private readonly object? value;
        public BadRequestActionResult(object? value)
        {
            this.value = value;
        }

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

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
}
