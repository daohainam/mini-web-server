using MiniWebServer.Models;
using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Controllers
{
    public class HelloWorldController : Controller
    {
        private readonly ISumCalculator sumCalculator;
        public HelloWorldController(ISumCalculator sumCalculator)
        {
            this.sumCalculator = sumCalculator;
        }

        public string Index(string? name)
        {
            return "Hello world! " + (name ?? string.Empty);
        }

        public async Task<int> SumAsync(int x, int y)
        {
            return await Task.FromResult(sumCalculator.Sum(x, y));
        }
        public IActionResult Profile(string name, int? yob)
        {
            return View(new ProfileModel() { Name = name ?? string.Empty, YearOfBirth = yob ?? 2000 });
        }
    }
}
