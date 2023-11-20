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
        public Func<IMiniAppContext, bool> RequestMatcher { get; set; } = DefaultRequestMatcher;

        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromMinutes(2);
        public IList<string> AllowedOrigins { get; } = new List<string>();

        private static bool DefaultRequestMatcher(IMiniAppContext appContext)
        {
            return true;
        }
    }
}
