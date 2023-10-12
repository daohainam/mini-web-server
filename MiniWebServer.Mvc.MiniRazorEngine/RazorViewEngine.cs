using Microsoft.Extensions.Logging;
using MiniWebServer.Mvc.Abstraction;
using MiniWebServer.Mvc.Abstraction.ViewContent;
using MiniWebServer.Mvc.MiniRazorEngine;
using MiniWebServer.Mvc.MiniRazorEngine.Parser;
using MiniWebServer.Mvc.MiniRazorEngine.Superpower;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class MiniRazorViewEngine: IViewEngine
    {
        public const string DefaultViewFolder = "Views";

        private readonly MiniRazorViewEngineOptions options;
        private readonly ILogger<MiniRazorViewEngine> logger;
        private readonly IViewFinder viewFinder;
        private readonly ITemplateParser templateParser;

        public MiniRazorViewEngine(MiniRazorViewEngineOptions options, ILogger<MiniRazorViewEngine> logger, IViewFinder? viewFinder = default, ITemplateParser? templateParser = default)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger;

            this.viewFinder = viewFinder ?? new DefaultViewFinder(DefaultViewFolder);
            this.templateParser = templateParser ?? new SuperpowerTemplateParser();
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