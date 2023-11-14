using MiniWebServer.MiniApp;

namespace MiniWebServer.Server.Host
{
    public class Host
    {
        public Host(string hostName, IMiniApp app)
        {
            HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            App = app ?? throw new ArgumentNullException(nameof(app));
        }

        public string HostName { get; }
        public IMiniApp App { get; }
    }
}
