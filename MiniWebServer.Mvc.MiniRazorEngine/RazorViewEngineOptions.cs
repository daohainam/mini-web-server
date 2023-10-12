using MiniWebServer.Mvc.MiniRazorEngine.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class MiniRazorViewEngineOptions
    {
        public string TempDirectory { get; set; } = ".tmp";
        public string AssembyCacheDirectory => Path.Combine(TempDirectory, "asmcache");
    }
}
