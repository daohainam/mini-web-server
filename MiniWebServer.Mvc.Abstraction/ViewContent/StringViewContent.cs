namespace MiniWebServer.Mvc.Abstraction.ViewContent
{
    public class StringViewContent(string content) : IViewContent
    {
        private readonly string content = content ?? string.Empty;

        public Task RenderAsync(ActionResultContext context)
        {
            context.Response.Content = new MiniApp.Content.StringContent(content);
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            return Task.CompletedTask;
        }
    }
}
