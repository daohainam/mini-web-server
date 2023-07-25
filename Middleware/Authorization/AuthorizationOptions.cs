using MiniWebServer.MiniApp.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authorization
{
    public class AuthorizationOptions
    {
        private readonly Dictionary<string, IPolicy> policies = new();

        public IDictionary<string, IPolicy> Policies => policies;
    }
}
