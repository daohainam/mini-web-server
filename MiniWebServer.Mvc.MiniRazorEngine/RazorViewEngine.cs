using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class MiniRazorViewEngine: IViewEngine
    {
        private readonly MiniRazorViewEngineOptions options;

        public MiniRazorViewEngine(MiniRazorViewEngineOptions options)
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