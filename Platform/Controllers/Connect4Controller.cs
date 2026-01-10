using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers;

public class Connect4Controller : Controller
{
    // Page d'accueil Puissance 4 (optionnelle)
    public IActionResult Index()
    {
        return View();
    }

    // POST /Connect4/Create
    // Appelé depuis un bouton "Créer une partie"
    [HttpPost]
    public IActionResult Create(string pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            TempData["Error"] = "Pseudo requis.";
            return RedirectToAction("Index");
        }

        var lobbyId = Guid.NewGuid().ToString("N");

        return RedirectToAction("Lobby", new { id = lobbyId, pseudo });
    }

    // GET /Connect4/Lobby/{id}?pseudo=...
    [HttpGet("/Connect4/Lobby/{id}")]
    public IActionResult Lobby(string id, string? pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            // fallback sécurité : on demandera le pseudo côté JS
            pseudo = "";
        }

        ViewBag.LobbyId = id;
        ViewBag.Pseudo = pseudo;

        return View();
    }
}
