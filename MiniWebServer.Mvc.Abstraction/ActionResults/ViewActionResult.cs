namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class ViewActionResult(ControllerContext controllerContext, string viewName, object? model, string contentType, IDictionary<string, object> viewData, IViewEngine viewEngine) : IActionResult
    {
        private readonly ControllerContext controllerContext = controllerContext ?? throw new ArgumentNullException(nameof(controllerContext));
        private readonly string viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
        private readonly string contentType = contentType ?? "text/html";
        private readonly IDictionary<string, object> viewData = viewData ?? new Dictionary<string, object>();
        private readonly IViewEngine viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));

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
