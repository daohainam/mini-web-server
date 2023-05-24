using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Quote
{
    public interface IQuoteService
    {
        Task<string> GetRandomAsync();
    }
}
