using GamingPlatform.Models;
using GamingPlatform.Data;
using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Controllers
{
    public class GameController : Controller
    {
        private readonly GameService _gameService;
        private readonly GamingPlatformContext _context;
        private readonly LobbyService _lobbyService;
        private readonly PlayerService _playerService;

        public GameController(GameService gameService, LobbyService lobbyService, PlayerService playerService, GamingPlatformContext context)
        {
            _gameService = gameService;
            _lobbyService = lobbyService;
            _playerService = playerService;
            context = context;

        }

        // Page d'accueil des jeux disponibles
        public IActionResult Index()
        {
            var games = _gameService.GetAvailableGames(); // Implémentez GetAvailableGames dans GameService
            return View(games);
        }

        // Affiche les détails d'un jeu en fonction de son ID
        public async Task<IActionResult> Details(Guid id)
        {
            var game = _gameService.GetGameById(id);
            if (game == null)
            {
                return NotFound("Le jeu spécifié n'existe pas.");
            }

            var lobbies = await _lobbyService.GetLobbiesByGameAsync(game.Code);

            var viewModel = new GameDetailsViewModel
            {
                Game = game,
                Lobbies = lobbies
            };
            int? playerId = null;
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                playerId = player.Id;
            }
            ViewBag.CurrentUserId = playerId;

            return View(viewModel);
        }

        // Affiche les lobbies d'un jeu en fonction de son code
        public async Task<IActionResult> LobbiesByGameCode(string gameCode)
        {
            var game = _gameService.GetGameByCode(gameCode);
            if (game == null)
            {
                return NotFound("Le jeu spécifié n'existe pas.");
            }

            var lobbies = await _lobbyService.GetLobbiesByGameAsync(gameCode);

            var viewModel = new GameDetailsViewModel
            {
                Game = game,
                Lobbies = lobbies
            };
            int? playerId = null;
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                playerId = player.Id;
            }
            ViewBag.CurrentUserId = playerId;

            return View(viewModel);
        }


        public async Task<Player> GetCurrentPlayer()
        {
            // R�cup�rer l'ID du joueur depuis la session
            var playerId = HttpContext.Session.GetInt32("PlayerId");

            if (playerId.HasValue)
            {
                return await _playerService.GetPlayerByIdAsync(playerId.Value);
            }

            return null;
        }

    }


}
