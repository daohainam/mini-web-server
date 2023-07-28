using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authorization
{
    public static class RouteDictionaryExtensions
    {
        public static void Add(this IDictionary<string, string[]> dictionary, string key, string value, params string[]? values)
        {
            List<string> strings = new()
            {
                value
            };

            if (values != null )
            {
                foreach (string v in values)
                {
                    strings.Add(v);
                }
            }
            dictionary[key] = strings.ToArray(); 
        }
    }
}
