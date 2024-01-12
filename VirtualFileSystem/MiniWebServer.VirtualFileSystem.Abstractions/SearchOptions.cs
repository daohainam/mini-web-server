using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.VirtualFileSystem.Abstractions
{
    public class SearchOptions
    {
        public bool SkipHidden { get; set; } = false;
        public bool SkipSystem { get; set; } = false;
    }
}
