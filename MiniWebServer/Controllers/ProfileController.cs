using MiniWebServer.Mvc.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Controllers
{
    public class ProfileController: Controller
    {
        public string Index()
        {
            return "Hello world!";
        } 
    }
}
