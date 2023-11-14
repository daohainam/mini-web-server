using MiniWebServer.Mvc.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Controllers
{
    public class FileController: Controller
    {
        [Route("/file")]
        public async Task<IActionResult> Content(string fileName)
        {
            var path = Path.Combine("wwwroot", "sample-texts", fileName);
            if (File.Exists(path))
            {
                var content = await File.ReadAllTextAsync(path);

                return Ok(content);
            }
            else
            {
                return NotFound($"{fileName} not found");
            }
        }
    }
}
