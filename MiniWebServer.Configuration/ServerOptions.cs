namespace MiniWebServer.Configuration
{
    public class ServerOptions
    {
        public HostOptions[] HostOptions { get; set; } = [];
        public BindingOptions[] BindingOptions { get; set; } = [];
        public ServerFeatureOptions FeatureOptions { get; set; } = new();
    }
}
