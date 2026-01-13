using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers;

public class Connect4Controller : Controller
{
    private const string SessionPseudoKey = "pseudo";

    // GET /Connect4/Index?lobbyId=...
    public IActionResult Index(string? lobbyId = null)
    {
        ViewBag.LobbyId = lobbyId ?? "";
        return View();
    }

    // POST /Connect4/Create  (crée une nouvelle partie)
    [HttpPost]
    public IActionResult Create(string pseudo)
    {
        pseudo = (pseudo ?? "").Trim();

        if (pseudo.Length < 2 || pseudo.Length > 20)
        {
            TempData["Error"] = "Pseudo requis (2 à 20 caractères).";
            return RedirectToAction("Index");
        }

        // ✅ stocker pseudo en session
        HttpContext.Session.SetString(SessionPseudoKey, pseudo);

        // ✅ créer nouveau lobby
        var lobbyId = Guid.NewGuid().ToString("N");

        // ✅ aller au lobby (sans pseudo dans l'URL)
        return RedirectToAction("Lobby", new { id = lobbyId });
    }

    // POST /Connect4/Join  (rejoint une partie existante)
    [HttpPost]
    public IActionResult Join(string lobbyId, string pseudo)
    {
        lobbyId = (lobbyId ?? "").Trim();
        pseudo = (pseudo ?? "").Trim();

        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            TempData["Error"] = "Lobby invalide.";
            return RedirectToAction("Index");
        }

        if (pseudo.Length < 2 || pseudo.Length > 20)
        {
            TempData["Error"] = "Pseudo requis (2 à 20 caractères).";
            return RedirectToAction("Index", new { lobbyId });
        }

        // ✅ stocker pseudo en session
        HttpContext.Session.SetString(SessionPseudoKey, pseudo);

        // ✅ rejoindre lobby existant
        return RedirectToAction("Lobby", new { id = lobbyId });
    }

    // GET /Connect4/Lobby/{id}
    [HttpGet("/Connect4/Lobby/{id}")]
    public IActionResult Lobby(string id)
    {
        id = (id ?? "").Trim();
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction("Index");

        var pseudo = (HttpContext.Session.GetString(SessionPseudoKey) ?? "").Trim();

        // ✅ si pas de pseudo -> demander via Index mais en gardant lobbyId
        if (pseudo.Length < 2)
            return RedirectToAction("Index", new { lobbyId = id });

        ViewBag.LobbyId = id;
        ViewBag.Pseudo = pseudo;

        return View(); // Views/Connect4/Lobby.cshtml
    }
}
