using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers;

public class Connect4Controller : Controller
{
    // /Connect4/Create
    public IActionResult Create()
    {
        var lobbyId = Guid.NewGuid().ToString("N");
        return RedirectToAction("Game", new { lobbyId });
    }

    // /Connect4/Game?lobbyId=...
    public IActionResult Game(string lobbyId)
    {
        ViewBag.LobbyId = lobbyId;
        return View();
    }
}
