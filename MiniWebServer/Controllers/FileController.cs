using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Controllers
{
    public class FileController : Controller
    {
        [Route("/file")]
        public IActionResult Content(string fileName)
        {
            var path = Path.Combine("wwwroot", "sample-texts", fileName);
            if (System.IO.File.Exists(path))
            {
                return File(new FileInfo(path));
            }
            else
            {
                return NotFound($"{fileName} not found");
            }
        }
    }
}
