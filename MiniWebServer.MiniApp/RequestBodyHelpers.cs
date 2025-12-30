using MiniWebServer.Abstractions;
using System.Text.Json;

namespace MiniWebServer.MiniApp;

public static class RequestBodyHelpers
{
    public static async Task<T?> ReadAsJsonAsync<T>(this IHttpRequest request, CancellationToken cancellationToken = default)
    {
        var jsonString = await request.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrEmpty(jsonString))
        {
            T? result = JsonSerializer.Deserialize<T?>(jsonString);

            return result;
        }
        else
        {
            return default;
        }
    }
}
