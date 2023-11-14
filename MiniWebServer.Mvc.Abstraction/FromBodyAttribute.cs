namespace MiniWebServer.Mvc.Abstraction
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromBodyAttribute : Attribute
    {
        public FromBodyAttribute() { }
    }
}
