using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKHeaderTable
    {
        public bool TryGetHeader(int index, out HPACKHeader? header)
        {
            if (index == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            else if (index <= HPACKStaticTable.TableSize) 
            {
                // since it is a static object, we have to clone a new one

                header = new HPACKHeader(HPACKStaticTable.GetHeader(index) ?? throw new Exception("header cannot be null"));
                return true;
            }
            else
            {
                throw new NotImplementedException("Dynamic table is not implemented yet! But soon! :)");

                //header = default;
                //return false;
            }
        }
    }
}
