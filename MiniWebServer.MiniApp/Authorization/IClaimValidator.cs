using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authorization
{
    public interface IClaimValidator
    {
        bool Validate(IMiniAppContext context);
    }
}
