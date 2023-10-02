using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MiniWebServer.MiniApp.Builders;
using MiniWebServer.Mvc;
using MiniWebServer.Mvc.Abstraction;
using MiniWebServer.Mvc.LocalAction;
using MiniWebServer.Mvc.RouteMatchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public static class MvcMiddlewareExtensions
    {
        public static void UseMvc(this IMiniAppBuilder appBuilder, MvcOptions? options = default)
        {
            if (options == null) {
                var registry = ScanLocalControllers();

                options = new MvcOptions(
                                new LocalActionFinder(), 
                                new RegexRouteMatcher()
                                );
            }

            appBuilder.Services.AddTransient(services => new MvcMiddleware(
                options,
                services.GetRequiredService<ILoggerFactory>(),
                appBuilder.Services
                ));

            appBuilder.UseMiddleware<MvcMiddleware>();
        }

        private static LocalActionRegistry ScanLocalControllers()
        {
            var registry = new LocalActionRegistry();

            var type = typeof(Controller);
            var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t != type);

            foreach ( var controllerType in controllerTypes)
            {
                var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach ( var action in actions)
                {
                    var attributes = action.GetCustomAttributes(false);

                    if (attributes != null )
                    {
                        if (attributes.Where(a => a is NonActionAttribute).Any())
                        {
                            // we skip NonAction methods
                            continue;
                        }
                        
                        var routeAttribute = attributes.Where(a => a is RouteAttribute).FirstOrDefault();
                        string route = routeAttribute != null ? ((RouteAttribute)routeAttribute).Route : $"/{controllerType.Name}/{action.Name}";

                        registry.Register(route, new LocalAction(
                            route,
                            new ActionInfo(
                                action.Name, action, controllerType
                                )
                            ));
                    }
                }
            }

            return registry;
        }
    }
}
