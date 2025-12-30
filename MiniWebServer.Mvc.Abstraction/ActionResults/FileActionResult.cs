using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ActionResults;

public class FileActionResult(FileInfo file) : IActionResult
{
    public Task ExecuteResultAsync(ActionResultContext context)
    {
        if (file.Exists)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            context.Response.Content = new MiniApp.Content.FileContent(file);
        }
        else
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.NotFound;
            context.Response.Content = MiniApp.Content.StringContent.Empty;
        }

        return Task.CompletedTask;
    }
}
