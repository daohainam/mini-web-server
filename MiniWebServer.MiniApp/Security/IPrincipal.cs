using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Security
{
    public interface IPrincipal
    {
        IIdentity? Identity { get; }
        bool IsInRole(string role);
    }
}
