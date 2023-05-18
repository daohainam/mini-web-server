using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IActionResult
    {
        int StatusCode { get; }
        string ReasonPhrase { get; }
    }
}
