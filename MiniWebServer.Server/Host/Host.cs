using MiniWebServer.MiniApp;

namespace MiniWebServer.Server.Host
{
    public class Host(string hostName, IMiniApp app)
    {
        public string HostName { get; } = hostName ?? throw new ArgumentNullException(nameof(hostName));
        public IMiniApp App { get; } = app ?? throw new ArgumentNullException(nameof(app));
    }
}
