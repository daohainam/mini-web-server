using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authorization;

namespace MiniWebServer.Authorization
{
    public class AuthorizationOptions
    {
        private readonly Dictionary<string, IPolicy> policies = new();
        private readonly Dictionary<string, string[]> routes = new();

        public IDictionary<string, IPolicy> Policies => policies;
        public IDictionary<string, string[]> Routes => routes;
        /// <summary>
        /// return true if you want to accept no-matching urls
        /// </summary>
        public Func<IMiniAppContext, bool> NoMatchedRoute { get; set; } = (context) => true;
    }
}
