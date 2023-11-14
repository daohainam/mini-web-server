namespace MiniWebServer.Mvc.Abstraction
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string route)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(route));

            Route = route;
        }
        public string Route { get; }
    }
}
