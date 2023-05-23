namespace MiniWebServer.Configuration
{
    public class BindingOptions
    {
        public int Port { get; set; } = 80;
        public string Address { get; set; } = "127.0.0.1"; // loop back address
        public string Certificate { get; set; } = string.Empty;
        public string CertificatePassword { get; set; } = string.Empty;
        public bool SSL { get; set; } = true;
    }
}