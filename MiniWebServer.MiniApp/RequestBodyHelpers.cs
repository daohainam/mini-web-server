using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
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
}
