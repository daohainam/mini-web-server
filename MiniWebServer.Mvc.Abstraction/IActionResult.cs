using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public interface IActionResult
    {
        Task ExecuteResultAsync(ActionResultContext context);
    }
}
