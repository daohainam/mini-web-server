using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public class SessionOptions
    {
        public const string DefaultSessionIdKey = ".miniWeb.SID";

        public SessionOptions(string? sessionIdKey = default)
        {
            SessionIdKey = sessionIdKey ?? DefaultSessionIdKey;
        }

        public string SessionIdKey { get; } = DefaultSessionIdKey;
    }
}
