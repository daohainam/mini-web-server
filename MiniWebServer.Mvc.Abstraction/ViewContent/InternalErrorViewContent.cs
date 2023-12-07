namespace MiniWebServer.Mvc.Abstraction.ViewContent
{
    public class InternalErrorViewContent(string content = "") : IViewContent
    {
        private readonly string content = content ?? string.Empty;

        public Task RenderAsync(ActionResultContext context)
        {
            context.Response.Content = new MiniApp.Content.StringContent(content);
            context.Response.StatusCode = Abstractions.HttpResponseCodes.InternalServerError;

            return Task.CompletedTask;
        }
    }
}
