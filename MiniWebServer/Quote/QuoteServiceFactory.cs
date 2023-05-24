using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Quote
{
    internal class QuoteServiceFactory
    {
        private static readonly IQuoteService service = new StaticQuoteService(); // StaticQuoteService or ZenquotesService 

        public static IQuoteService GetQuoteService() => service;
    }
}
