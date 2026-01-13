using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers
{
    public class SpeedTypingController : Controller
    {
        private readonly LobbyService _lobbyService;

        public SpeedTypingController(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        // PAGE D’ACCUEIL / JEU (utilise View SpeedTyping/Index)
        public IActionResult Index()
        {
            return View();
        }

        // CRÉATION DU LOBBY (option A)
        [HttpPost]
        public IActionResult Create(string pseudo, int maxPlayers = 4)
        {
            if (string.IsNullOrWhiteSpace(pseudo))
            {
                TempData["Error"] = "Pseudo obligatoire.";
                return RedirectToAction("Index");
            }

            if (maxPlayers < 2) maxPlayers = 2;

            var lobby = _lobbyService.CreateLobby("SpeedTyping", maxPlayers);

            return RedirectToAction(
                actionName: "Lobby",
                routeValues: new { id = lobby.Id, pseudo }
            );
        }

        [HttpGet("/SpeedTyping/Lobby/{id}")]
        public IActionResult Lobby(string id, string? pseudo)
        {
            if (string.IsNullOrWhiteSpace(pseudo))
            {
                // affiche Join pour demander le pseudo
                ViewBag.LobbyId = id;
                return View("Join");
            }

            ViewBag.LobbyId = id;
            ViewBag.Pseudo = pseudo;
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ViewBag.LobbyUrl = $"{baseUrl}/SpeedTyping/Lobby/{id}";

            return View("Game");
        }

    }
}