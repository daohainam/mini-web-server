using Microsoft.Extensions.Logging;
using MiniWebServer.Mvc.Abstraction;
using MiniWebServer.Mvc.Abstraction.ViewContent;
using MiniWebServer.Mvc.MiniRazorEngine;
using MiniWebServer.Mvc.MiniRazorEngine.Parser;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class MiniRazorViewEngine: IViewEngine
    {
        public const string DefaultViewFolder = "Views";

        private readonly MiniRazorViewEngineOptions options;
        private readonly ILogger<MiniRazorViewEngine> logger;
        private readonly IViewFinder viewFinder;
        private readonly ITemplateParser templateParser;

        public MiniRazorViewEngine(MiniRazorViewEngineOptions options, ILogger<MiniRazorViewEngine> logger, ITemplateParser templateParser, IViewFinder? viewFinder = default)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
            this.logger = logger;

            this.viewFinder = viewFinder ?? new DefaultViewFinder(DefaultViewFolder);
        }

        public async Task<IViewContent?> RenderAsync(ActionResultContext context, string viewName, object? model, IDictionary<string, object> viewData)
        {
            try
            {
                var template = viewFinder.Find(context, viewName);
                if (template == null)
                {
                    logger.LogError("View not found: {v}", viewName);
                    return await Task.FromResult(new InternalErrorViewContent());
                }

                if (templateParser.TryParse(template, out var generatedSource) && generatedSource != null)
                {
                    string builtContent = generatedSource;

                    return await Task.FromResult(new StringViewContent(builtContent));
                }
                else
                {
                    logger.LogError("Error parsing view: {v}", viewName);
                    return await Task.FromResult(new InternalErrorViewContent());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rendering view");
                return await Task.FromResult(new InternalErrorViewContent(ex.Message + "\r\n" + ex.StackTrace));
            }
        }
    }
}