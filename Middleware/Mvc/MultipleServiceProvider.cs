namespace MiniWebServer.Mvc
{
    internal class MultipleServiceProvider(IServiceProvider parent1, params IServiceProvider[] parents) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            object? service = parent1.GetService(serviceType);
            if (service == null && parents.Length > 0)
            {
                foreach (var parent in parents)
                {
                    service = parent.GetService(serviceType);
                    if (service != null)
                    {
                        break;
                    }
                }
            }

            return service;
        }
    }
}
