using System.Text.Json;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class JsonActionResult : IActionResult
    {
        private readonly object value;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        public JsonActionResult(object value, JsonSerializerOptions? jsonSerializerOptions)
        {
            this.value = value;
            this.jsonSerializerOptions = jsonSerializerOptions ?? JsonSerializerOptions.Default;
        }

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            var valueString = JsonSerializer.Serialize(value, jsonSerializerOptions);
            context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);

            return Task.CompletedTask;
        }
    }
}
