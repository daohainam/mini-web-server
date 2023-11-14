using Microsoft.Extensions.DependencyInjection;

namespace MiniWebServer.MiniApp.Builders
{
    public class MiniAppBuilder : IMiniAppBuilder
    {
        public IServiceCollection Services { get; }

        private readonly List<Type> middlewareTypes = new();

        public MiniAppBuilder(IServiceCollection? services = default)
        {
            if (services == null)
            {
                Services = new ServiceCollection();
            }
            else
            {
                Services = services;
            }
        }

        public virtual IMiniApp Build()
        {
            var services = Services.BuildServiceProvider();
            var middlewares = new List<IMiddleware>();

            if (middlewareTypes.Any())
            {
                var middlewareFactory = services.GetService<IMiddlewareFactory>();

                foreach (var type in middlewareTypes)
                {
                    IMiddleware? middleware = null;

                    if (middlewareFactory != null)
                    {
                        middleware = middlewareFactory.Create(type);
                    }

                    middleware ??= services.GetService(type) as IMiddleware;

                    if (middleware == null)
                    {
                        throw new InvalidOperationException($"Middleware not registered: {type}");
                    }

                    middlewares.Add(middleware);
                }
            }

            var app = new BaseMiniApp(services, middlewares);
            return app;
        }

        public IMiniAppBuilder UseMiddleware(Type middlewareType)
        {
            ArgumentNullException.ThrowIfNull(middlewareType);

            if (typeof(IMiddleware).IsAssignableFrom(middlewareType))
            {
                middlewareTypes.Insert(0, middlewareType); // LIFO: last added middleware will be called first
            }
            else
            {
                throw new InvalidOperationException("middlewareType must be a IMiddleware");
            }

            return this;
        }

        public IMiniAppBuilder UseMiddleware<TMiddleware>()
        {
            return UseMiddleware(typeof(TMiddleware));
        }
    }
}
