using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authorization
{
    public class RegExRouteMatcher : IRouteMatcher
    {
        public bool IsMatched(string routeUrl, string url)
        {
            // we will replace with RegEx later

            return string.Compare(routeUrl, url, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
