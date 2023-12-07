namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class OkActionResult(object? value) : IActionResult
    {
        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            if (value != null)
            {
                if (value.GetType() == typeof(string))
                {
                    context.Response.Content = MiniApp.Content.StringContent.FromValue((string)value);
                }
                else
                {
                    var valueString = value.ToString();
                    context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);
                }
            }
            else
            {
                context.Response.Content = MiniApp.Content.StringContent.Empty;
            }

            return Task.CompletedTask;
        }
    }
}
