namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class ViewActionResult : IActionResult
    {
        private readonly ControllerContext controllerContext;
        private readonly string viewName;
        private readonly string contentType;
        private readonly object? model;
        private readonly IDictionary<string, object> viewData;
        private readonly IViewEngine viewEngine;

        public ViewActionResult(ControllerContext controllerContext, string viewName, object? model, string contentType, IDictionary<string, object> viewData, IViewEngine viewEngine)
        {
            this.controllerContext = controllerContext ?? throw new ArgumentNullException(nameof(controllerContext));
            this.viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
            this.model = model;
            this.contentType = contentType ?? "text/html";
            this.viewData = viewData ?? new Dictionary<string, object>();
            this.viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        }

        public async Task ExecuteResultAsync(ActionResultContext context)
        {
            var content = await viewEngine.RenderAsync(context, viewName, model, viewData);

            if (content != null)
            {
                await content.RenderAsync(context);
            }
            else
            {
                controllerContext.Context.Response.Content = new MiniApp.Content.StringContent(string.Empty);
                controllerContext.Context.Response.StatusCode = Abstractions.HttpResponseCodes.InternalServerError;
            }
        }
    }
}
