using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class RazorViewEngine: IViewEngine
    {
        private readonly RazorViewEngineOptions options;

        public RazorViewEngine(RazorViewEngineOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        Task<bool> IViewEngine.RenderAsync(string viewName, object? model, IDictionary<string, object> viewData, out string? renderedContent)
        {
            renderedContent = "Content rendered by RazorViewEngine, viewName = " + viewName;

            return Task.FromResult(true);
        }
    }
}