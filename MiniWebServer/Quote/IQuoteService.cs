namespace MiniWebServer.Quote
{
    public interface IQuoteService
    {
        Task<string> GetRandomAsync();
    }
}
