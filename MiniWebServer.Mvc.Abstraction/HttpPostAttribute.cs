using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPostAttribute : Attribute
    {
    }
}
