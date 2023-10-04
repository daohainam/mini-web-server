using MiniWebServer.Mvc.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Controllers
{
    public class HelloWorldController: Controller
    {
        public string Index(string? name)
        {
            return "Hello world! " + (name ?? string.Empty);
        } 
    }
}
