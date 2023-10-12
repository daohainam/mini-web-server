using MiniWebServer.Mvc.Abstraction;
using MiniWebServer.Mvc.Abstraction.ViewContent;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class MiniRazorViewEngine: IViewEngine
    {
        private readonly MiniRazorViewEngineOptions options;

        public MiniRazorViewEngine(MiniRazorViewEngineOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IViewContent?> RenderAsync(string viewName, object? model, IDictionary<string, object> viewData)
        {
            return await Task.FromResult(new StringViewContent("Content rendered by RazorViewEngine, viewName = " + viewName));
        }
    }
}