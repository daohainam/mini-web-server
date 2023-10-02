using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.RouteMatchers
{
    internal class RegexRouteMatcher : IRouteMatcher
    {
        public bool IsMatched(string requestUrl, string route)
        {
            return string.Equals(requestUrl, route, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
