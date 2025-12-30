using MiniWebServer.MiniApp;

namespace MiniWebServer.Server;

public class HostConfiguration(string hostName, IMiniApp app)
{
    public string HostName { get; } = hostName;
    public IMiniApp App { get; } = app;
}
