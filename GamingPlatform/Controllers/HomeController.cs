using GamingPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GamingPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public IActionResult SpeedTyping()
        {
            // Affiche la page SpeedTyping avec juste l’UI
            return View("~/Views/SpeedTyping/index.cshtml");
        }
        
        public IActionResult Login()
        {
            return View("~/Views/Login/index.cshtml");
        }
        public IActionResult Lobby()
        {
            return View("~/Views/Lobby/index.cshtml");
        }
    }
}
