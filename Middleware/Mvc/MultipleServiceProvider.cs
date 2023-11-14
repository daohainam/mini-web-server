using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc
{
    internal class MultipleServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider parent1;
        private readonly IServiceProvider[] parents;

        public MultipleServiceProvider(IServiceProvider parent1, params IServiceProvider[] parents)
        {
            this.parent1 = parent1;
            this.parents = parents;
        }

        public object? GetService(Type serviceType)
        {
            object? service = parent1.GetService(serviceType);
            if (service == null && parents.Length > 0)
            {
                foreach (var parent in parents)
                {
                    service = parent.GetService(serviceType);
                    if (service != null)
                    {
                        break;
                    }
                }
            }

            return service;
        }
    }
}
