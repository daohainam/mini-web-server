using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.RouteMatchers
{
    internal class RegexRouteMatcher : IRouteMatcher
    {
        // todo: we should accept a regex and match requests based on that
        public bool IsMatched(string requestUrl, string route)
        {
            return string.Equals(requestUrl, route, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
