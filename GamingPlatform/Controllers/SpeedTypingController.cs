using GamingPlatform.Models;
using Microsoft.AspNetCore.Mvc;

public class SpeedTypingController : Controller
{
    public IActionResult Index()
    {
        var model = new TypingGameViewModel
        {
            
            TextToType = "Practice makes perfect. The quick brown fox jumps over the lazy dog.",
            TimeLimitSeconds = 60,
            Progress = 0
        };
        return View(model);  // ✅ Passe le model, pas null
    }
}