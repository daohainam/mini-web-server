using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpParameters : Dictionary<string, HttpParameter>
    {
        public HttpParameters()
        {
        }

        public HttpParameters(HttpParameters others)
        {
            foreach (var item in others.Values)
            {
                if (ContainsKey(item.Name))
                {
                    this[item.Name].AddValue(item.Value ?? string.Empty);
                }
                else
                {
                    Add(item.Name, item);
                }
            }
        }


        public HttpParameters(IEnumerable<HttpParameter> collection) 
        {
            foreach (var item in collection)
            {
                if (ContainsKey(item.Name))
                {
                    this[item.Name].AddValue(item.Value ?? string.Empty);
                }
                else
                {
                    Add(item.Name, item);
                }
            }
        }

        public void Add(HttpParameter item)
        {
            if (ContainsKey(item.Name))
            {
                this[item.Name].AddValue(item.Value ?? string.Empty);
            }
            else
            {
                Add(item.Name, item);
            }
        }
    }
}
