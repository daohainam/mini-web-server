using Microsoft.Extensions.Caching.Distributed;
using MiniWebServer.MiniApp;

namespace Session
{
    public class SessionMiddleware : IMiddleware
    {
        public const string DefaultSessionIdKey = ".mini.SID";
        private readonly string sessionIdKey;
        private readonly ISessionStore sessionStore;

        public SessionMiddleware(string sessionIdKey, ISessionStore sessionStore)
        {
            this.sessionIdKey = string.IsNullOrEmpty(sessionIdKey) ? DefaultSessionIdKey : sessionIdKey;
            this.sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            string? sessionId = null;
            if (context.Request.Cookies.TryGetValue(sessionIdKey, out var cookie) && cookie != null)
            {
                sessionId = cookie.Value;

                if (IsValidSessionId(sessionId))
                {
                    var session = await sessionStore.FindOrCreateAsync(sessionId, cancellationToken);

                    // we store session data in a dictionary
                    if (session != null)
                    {
                        context.Session = session;
                    }
                }
                else
                {
                    sessionId = null;
                }
            }

            // if sessionId == null, we create a new session
        }

        private bool IsValidSessionId(string sessionId)
        {
            return !string.IsNullOrEmpty(sessionId); // at least not empty
        }
    }
}