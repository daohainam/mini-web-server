using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class MiniWebServerBindingConfiguration
    {
        public MiniWebServerBindingConfiguration(IPEndPoint httpEndPoint, X509Certificate2? certificate = null)
        {
            HttpEndPoint = httpEndPoint ?? throw new ArgumentNullException(nameof(httpEndPoint));
            Certificate = certificate;
        }

        public IPEndPoint HttpEndPoint { get; }
        public X509Certificate2? Certificate { get; }
    }
}
