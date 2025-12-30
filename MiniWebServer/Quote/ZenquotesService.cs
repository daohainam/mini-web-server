using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MiniWebServer.Quote;

public class ZenquotesService : IQuoteService
{
    private readonly HttpClient httpClient = new();


    public async Task<string> GetRandomAsync()
    {
        try
        {
            var q = await httpClient.GetStringAsync("https://zenquotes.io/api/random");

            var quotes = await httpClient.GetFromJsonAsync<QuoteRecord[]>("https://zenquotes.io/api/random");

            if (quotes == null || quotes.Length == 0)
            {
                return "Error getting quote";
            }
            else
            {
                var quote = quotes.First();
                return $"\"{quote.Quote}\" - {quote.Author} (From https://zenquotes.io)" ?? "Quote record error";
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private class QuoteRecord
    {
        [JsonPropertyName("q")]
        public string? Quote { get; set; }
        [JsonPropertyName("a")]
        public string? Author { get; set; }
        [JsonPropertyName("h")]
        public string? Html { get; set; }
    }
}
