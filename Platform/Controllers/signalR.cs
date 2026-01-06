using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers
{
    public class signalR : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
