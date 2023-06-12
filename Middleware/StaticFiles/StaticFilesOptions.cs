using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.StaticFiles
{
    public class StaticFilesOptions
    {
        public string Root { get; set; } = "wwwroot";
        public string[] DefaultDocuments { get; set; } = new string[] { "index.htm", "index.html" };
    }
}
