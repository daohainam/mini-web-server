using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Mvc;
using MiniWebServer.Mvc.Abstraction;
using MiniWebServer.Mvc.LocalAction;
using MiniWebServer.Mvc.RazorEngine;
using MiniWebServer.Mvc.RazorLightTemplateParser;
using MiniWebServer.Mvc.RouteMatchers;
using System.Reflection;
using System.Xml.Linq;

namespace MiniWebServer.Session
{
    public static class MvcMiddlewareExtensions
    {
        public static void AddMvcService(this IServiceCollection services, Action<MvcOptions>? configureOptions = default)
        {
            var registry = ScanLocalControllers();
            var routeMatcher = new StringEqualsRouteMatcher();

            var options = new MvcOptions(
                                new LocalActionFinder(registry, routeMatcher)
                                );

            configureOptions?.Invoke(options);

            services.AddTransient<IViewEngine>(
                services => new MiniRazorViewEngine(
                                    new MiniRazorViewEngineOptions(),
                                    services.GetRequiredService<ILogger<MiniRazorViewEngine>>(),
                                    new RazorLightTemplateParser()
                                    )
                );

            services.AddTransient(services => new MvcMiddleware(
                options,
                services.GetRequiredService<IViewEngine>(),
                services.GetRequiredService<ILoggerFactory>(),
                services
                ));
        }

        public static void UseMvc(this IMiniAppBuilder appBuilder, Action<MvcOptions>? configureOptions = default)
        {
            appBuilder.UseMiddleware<MvcMiddleware>();
        }

        private static LocalActionRegistry ScanLocalControllers()
        {
            var registry = new LocalActionRegistry();

            var type = typeof(Controller);
            var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t != type);

            foreach (var controllerType in controllerTypes)
            {
                var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var action in actions)
                {
                    var actionParameters = action.GetParameters();
                    bool isValid = true;
                    foreach (var parameter in actionParameters)
                    {
                        if (parameter.IsOut)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        var attributes = action.GetCustomAttributes(false);

                        if (attributes != null)
                        {
                            if (attributes.Where(a => a is NonActionAttribute).Any())
                            {
                                // we skip NonAction methods
                                continue;
                            }

                            var routeAttribute = attributes.Where(a => a is RouteAttribute).FirstOrDefault();
                            var controllerTypeName = controllerType.Name;
                            if (controllerTypeName.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
                            {
                                controllerTypeName = controllerTypeName[..^10];
                            }

                            string route;
                            if (routeAttribute != null)
                            {
                                route = ((RouteAttribute)routeAttribute).Route;
                            }
                            else
                            {
                                string actionName = action.Name;
                                // is this an async function, we will remove the postfix
                                if (actionName.EndsWith("Async", StringComparison.InvariantCultureIgnoreCase) && action.ReturnType.IsGenericType && action.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                                {
                                    actionName = actionName[..^5];
                                }
                                route = $"/{controllerTypeName}/{actionName}";
                            }

                            var methods = ActionMethods.None;
                            foreach (var attr in attributes)
                            {
                                if (attr is HttpGetAttribute)
                                {
                                    methods |= ActionMethods.Get;
                                }
                                else if (attr is HttpPutAttribute)
                                {
                                    methods |= ActionMethods.Put;
                                }
                                else if (attr is HttpPostAttribute)
                                {
                                    methods |= ActionMethods.Post;
                                }
                                else if (attr is HttpOptionsAttribute)
                                {
                                    methods |= ActionMethods.Options;
                                }
                                else if (attr is HttpDeleteAttribute)
                                {
                                    methods |= ActionMethods.Delete;
                                }
                                else if (attr is HttpHeadAttribute)
                                {
                                    methods |= ActionMethods.Head;
                                }
                            }

                            // if no Http* attribute defined, we support all
                            if (methods == ActionMethods.None)
                            {
                                methods = ActionMethods.All;
                            }

                            registry.Register(route, new LocalAction(
                                route,
                                new ActionInfo(
                                    action.Name, action, controllerType
                                    ),
                                methods
                                ));
                        }
                    }
                }
            }

            return registry;
        }
    }
}
