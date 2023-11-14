namespace MiniWebServer.Quote
{
    internal class QuoteServiceFactory
    {
        private static readonly IQuoteService service = new StaticQuoteService(); // StaticQuoteService or ZenquotesService 

        public static IQuoteService GetQuoteService() => service;
    }
}
