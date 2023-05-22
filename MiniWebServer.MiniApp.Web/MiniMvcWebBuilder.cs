using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web
{
    public class MiniMvcWebBuilder : MiniWebBuilder, IMiniMvcWebBuilder
    {
        private bool useMvc = false;

        public MiniMvcWebBuilder(ILogger<MiniMvcWebBuilder> logger) : base(logger)
        {
        }

        public IMiniMvcWebBuilder UseMvc()
        {
            useMvc = true;

            return this;
        }

        public override MiniWeb Build()
        {
            var app = base.Build();

            return app;
        }
    }
}
