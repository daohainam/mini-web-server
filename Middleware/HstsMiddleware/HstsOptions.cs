using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.HstsMiddleware
{
    public class HstsOptions
    {
        public int MaxAge { get; set; } = 63072000; // two years is recommended: https://hstspreload.org/
        public bool IncludeSubDomains { get; set; } = true;
        public bool Preload { get; set; } = true;
    }
}
