using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authorization
{
    public static class PolicyDictionaryExtensions
    {
        public static void Add(this IDictionary<string, IPolicy> policies, string name, Action<IPolicy> action)
        {
            var policy = new Policy();
            action(policy);

            policies.Add(name, policy);
        }
    }
}
