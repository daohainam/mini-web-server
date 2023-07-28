using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authorization
{
    public interface IRouteMatcher
    {
        bool IsMatched(string routeUrl, string url);
    }
}
