using MiniWebServer.MiniApp;
using MiniWebServer.Mvc.Abstraction;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Mvc.LocalAction
{
    internal class LocalActionFinder : IActionFinder
    {
        private readonly LocalActionRegistry registry;
        private readonly IRouteMatcher routeMatcher;

        public LocalActionFinder(LocalActionRegistry registry, IRouteMatcher routeMatcher)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.routeMatcher = routeMatcher ?? throw new ArgumentNullException(nameof(routeMatcher));
        }
        public ActionInfo? Find(IMiniAppContext context)
        {
            var key = registry.Actions.Keys.Where(k => routeMatcher.IsMatched(context.Request.Url, k)).FirstOrDefault();
            if (key != null)
            {
                if (registry.Actions.TryGetValue(key, out LocalAction? localAction))
                {
                    if (((localAction.ActionMethods & ActionMethods.Get) == ActionMethods.Get) && context.Request.Method == HttpMethod.Get)
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Post) == ActionMethods.Post) && context.Request.Method.Equals(HttpMethod.Post))
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Put) == ActionMethods.Put) && context.Request.Method.Equals(HttpMethod.Put))
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Delete) == ActionMethods.Delete) && context.Request.Method.Equals(HttpMethod.Delete))
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Patch) == ActionMethods.Patch) && context.Request.Method.Equals(HttpMethod.Patch))
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Head) == ActionMethods.Head) && context.Request.Method.Equals(HttpMethod.Head))
                    {
                        return localAction.ActionInfo;
                    }
                    if (((localAction.ActionMethods & ActionMethods.Options) == ActionMethods.Options) && context.Request.Method.Equals(HttpMethod.Options))
                    {
                        return localAction.ActionInfo;
                    }
                }
            }

            return null;
        }
    }
}
