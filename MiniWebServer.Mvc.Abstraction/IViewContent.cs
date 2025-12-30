namespace MiniWebServer.Mvc.Abstraction;

public interface IViewContent
{
    Task RenderAsync(ActionResultContext context);
}
