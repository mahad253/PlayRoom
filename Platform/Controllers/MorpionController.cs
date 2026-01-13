using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers
{
    public class MorpionController : Controller
    {
        private readonly LobbyService _lobbyService;

        public MorpionController(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        // =======================
        // PAGE D’ACCUEIL MORPION
        // =======================
        public IActionResult Index()
        {
            return View();
        }

        // =======================
        // CRÉATION DU LOBBY
        // =======================
        [HttpPost]
        public IActionResult Create(string pseudo)
        {
            // Ici, le pseudo est OBLIGATOIRE (créateur)
            if (string.IsNullOrWhiteSpace(pseudo))
            {
                TempData["Error"] = "Pseudo obligatoire.";
                return RedirectToAction("Index");
            }

            var lobby = _lobbyService.CreateLobby("Morpion", maxPlayers: 2);

            // On passe le pseudo UNIQUEMENT pour le créateur
            return RedirectToAction(
                actionName: "Lobby",
                routeValues: new { id = lobby.Id, pseudo }
            );
        }

        // =======================
        // PAGE LOBBY (LIEN PUBLIC)
        // =======================
        [HttpGet("/Morpion/Lobby/{id}")]
        public IActionResult Lobby(string id, string? pseudo)
        {
            // ⚠️ ICI, PLUS AUCUNE VALIDATION DU PSEUDO
            // Le pseudo peut être vide → JS s’en occupe

            ViewBag.LobbyId = id;
            ViewBag.Pseudo = pseudo ?? ""; // vide si invité

            return View();
        }
    }
}
