namespace MiniWebServer.Mvc.Abstraction
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromHeaderAttribute : Attribute
    {
        public FromHeaderAttribute() { }
    }
}
