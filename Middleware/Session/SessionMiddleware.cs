using MiniWebServer.MiniApp;

namespace MiniWebServer.Session
{
    public class SessionMiddleware : IMiddleware
    {
        private readonly SessionOptions options;
        private readonly ISessionIdGenerator sessionIdGenerator;
        private readonly ISessionStore sessionStore;

        public SessionMiddleware(SessionOptions? options, ISessionIdGenerator? sessionIdGenerator, ISessionStore? sessionStore)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.options = options;
            this.sessionIdGenerator = sessionIdGenerator ?? throw new ArgumentNullException(nameof(sessionIdGenerator));
            this.sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            if (context.Request.Cookies.TryGetValue(options.SessionIdKey, out var cookie) && cookie != null)
            {
                var sessionId = cookie.Value;

                if (IsValidSessionId(sessionId))
                {
                    var session = sessionStore.Create(sessionId);

                    // we store session data in a dictionary
                    if (session != null)
                    {
                        context.Session = session;
                    }

                    context.Response.Cookies.Add(
                        options.SessionIdKey,
                        new Abstractions.Http.HttpCookie(options.SessionIdKey, sessionId)
                        );
                }
                else
                {
                    sessionId = null;
                }
            }
            else // session key not found, create one
            {
                context.Response.Cookies.Add(
                    options.SessionIdKey,
                    new Abstractions.Http.HttpCookie(
                        options.SessionIdKey, 
                        sessionIdGenerator.GenerateNewId(),
                        httpOnly: true,
                        path: "/"
                        )
                    );
            }

            await next.InvokeAsync(context, cancellationToken);
        }

        private static bool IsValidSessionId(string sessionId)
        {
            return !string.IsNullOrEmpty(sessionId); // at least not empty
        }
    }
}