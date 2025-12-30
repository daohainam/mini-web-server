namespace MiniWebServer.Mvc.Abstraction;

public interface IViewEngine
{
    // generate content data from a model using viewName to find templates
    Task<IViewContent?> RenderAsync(ActionResultContext context, string viewName, object? model, IDictionary<string, object> viewData);
}
