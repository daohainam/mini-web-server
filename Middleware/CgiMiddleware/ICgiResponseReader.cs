using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi;

public interface ICgiResponseReader
{
    Task<CgiResponse?> ReadAsync();
}
