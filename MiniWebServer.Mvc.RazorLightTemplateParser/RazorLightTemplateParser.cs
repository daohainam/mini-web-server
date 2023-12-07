using MiniWebServer.Mvc.MiniRazorEngine.Parser;
using RazorLight;

namespace MiniWebServer.Mvc.RazorLightTemplateParser
{
    public class RazorLightTemplateParser : ITemplateParser
    {
        private readonly RazorLightTemplateParserOptions options;
        private static readonly string[] DefaultNamespaces = [
            "System",
            "System.Text"
        ];

        public RazorLightTemplateParser(RazorLightTemplateParserOptions? options = null)
        {
            this.options = options ?? new RazorLightTemplateParserOptions()
            {
                DefaultNamespaces = DefaultNamespaces
            };
        }

        public async Task<ParseResult> ParseAsync(string viewName, string template, object? model)
        {
            try
            {
                var engine = LoadEngine();

                var sourceCode = await engine.CompileRenderStringAsync(viewName, template, model);

                return new ParseResult(true, sourceCode);
            }
            catch (Exception ex)
            {
                return new ParseResult(false, ex);
            }
        }

        private RazorLightEngine LoadEngine()
        {
            var engine = new RazorLightEngineBuilder()
                .AddDefaultNamespaces(options.DefaultNamespaces)
                .EnableDebugMode()
                .UseMemoryCachingProvider()
                .Build();

            return engine;
        }
    }
}
