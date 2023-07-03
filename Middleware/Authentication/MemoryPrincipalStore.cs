using MiniWebServer.MiniApp.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public class MemoryPrincipalStore : IPrincipalStore
    {
        private readonly ConcurrentDictionary<string, IPrincipal> principals = new();

        public IPrincipal? GetPrincipal(string key)
        {
            if (principals.TryGetValue(key, out IPrincipal? principal)) { 
                return principal; 
            }

            return null;
        }

        public bool SetPrincipal(string key, IPrincipal principal)
        {
            return principals.TryAdd(key, principal);
        }
    }
}
