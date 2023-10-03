using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.LocalAction
{
    [Flags]
    internal enum ActionMethods // there is no support for Connect method since it is proxy-related
    {
        None = 0b_0000_0000,  // 0
        Get = 0b_0000_0001,  // 1
        Head = 0b_0000_0010,  // 2
        Post = 0b_0000_0100,  // 4
        Put = 0b_0000_1000,  // 8
        Delete = 0b_0001_0000,  // 16
        Options = 0b_010_0000,  // 32
        Trace = 0b_0100_0000,  // 64
        Patch = 0b_1000_0000,  // 128
        GetAndPost = Get | Post,
        All = Get | Head | Post | Put | Delete | Options | Trace | Patch
    }
}
