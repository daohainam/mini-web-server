using MiniWebServer.MiniApp;

namespace MiniWebServer.Session;

public interface ISessionStore
{
    ISession Create(string sessionId);
}
