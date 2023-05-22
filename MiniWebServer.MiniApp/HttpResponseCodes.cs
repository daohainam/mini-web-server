using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public class HttpResponseCodes
    {
        public const int Ok = 200;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int InternalServerError = 500;
    }
}
