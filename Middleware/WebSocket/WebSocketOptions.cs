using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    public class WebSocketOptions
    {
        // Mini-Web-Server is not designed to very secure, so by default we accept any request and origin header
        public Func<IMiniAppRequestContext, bool> RequestMatcher { get; set; } = DefaultRequestMatcher; // return true to allow the request 
        public Func<string, bool> OriginValidator { get; set; } = DefaultOriginValidator; // return true to accept the Origin header value

        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromMinutes(2);
        public string? SubProtocol { get; set; }
        public IList<string> AllowedOrigins { get; } = new List<string>();

        private static bool DefaultRequestMatcher(IMiniAppRequestContext appContext)
        {
            return true;
        }
        private static bool DefaultOriginValidator(string origin) 
        {
            return true;
        }
    }
}
