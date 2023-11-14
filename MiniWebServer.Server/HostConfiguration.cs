using MiniWebServer.MiniApp;

namespace MiniWebServer.Server
{
    public class HostConfiguration
    {
        public HostConfiguration(string hostName, IMiniApp app)
        {
            HostName = hostName;
            App = app;
        }

        public string HostName { get; }
        public IMiniApp App { get; }
    }
}
