using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class MiniWebServerBindingConfiguration(IPEndPoint httpEndPoint, X509Certificate2? certificate = null)
    {
        public IPEndPoint HttpEndPoint { get; } = httpEndPoint ?? throw new ArgumentNullException(nameof(httpEndPoint));
        public X509Certificate2? Certificate { get; } = certificate;
    }
}
