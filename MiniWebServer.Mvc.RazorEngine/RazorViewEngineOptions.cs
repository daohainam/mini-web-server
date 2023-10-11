using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.RazorEngine
{
    public class RazorViewEngineOptions
    {
        public string ViewDirectory { get; set; } = "Views";
        public string TempDirectory { get; set; } = ".tmp";
        public string AssembyCacheDirectory => Path.Combine(TempDirectory, "asmcache");
    }
}
