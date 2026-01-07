using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers
{
    public class MorpionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
