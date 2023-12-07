using System.Text.Json;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class JsonActionResult(object value, JsonSerializerOptions? jsonSerializerOptions) : IActionResult
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions ?? JsonSerializerOptions.Default;

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            var valueString = JsonSerializer.Serialize(value, jsonSerializerOptions);
            context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);

            return Task.CompletedTask;
        }
    }
}
