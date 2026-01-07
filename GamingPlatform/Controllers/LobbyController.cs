﻿using Microsoft.AspNetCore.Mvc;
using GamingPlatform.Services;
using GamingPlatform.Models;

using GamingPlatform.Data;


namespace GamingPlatform.Controllers
{
    public class LobbyController : Controller
    {
        private readonly LobbyService _lobbyService;
        private readonly GameService _gameService;
        private readonly PlayerService _playerService;
        private readonly GamingPlatformContext _context;

        public LobbyController(LobbyService lobbyService, GameService gameService, PlayerService playerService, GamingPlatformContext context)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;
            _playerService = playerService;
            _context = context;
        }

        public async Task<IActionResult> Index(string? name, string? gameCode, LobbyStatus? status)
        {
            // Charger les lobbies
            var lobbies = await _lobbyService.GetAllLobbies();

            // Appliquer les filtres
            if (!string.IsNullOrEmpty(name))
            {
                lobbies = lobbies.Where(l => l.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(gameCode))
            {
                lobbies = lobbies.Where(l => l.GameCode == gameCode);
            }

            if (status.HasValue)
            {
                lobbies = lobbies.Where(l => l.Status == status.Value);
            }

            // Charger les jeux pour les filtres
            ViewBag.Games = _gameService.GetAvailableGames();
 
            int ? playerId = null;
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                playerId = player.Id;
            }
            ViewBag.CurrentUserId = playerId;
            return View(lobbies);
        }

        public IActionResult Details(Guid id)
        {
            var lobby = _lobbyService.GetLobbyWithGameAndPlayers(id);
            if (lobby == null)
            {
                return NotFound();
            }

            return View(lobby);
        }


        // Affiche un formulaire avec un select pour choisir un jeu
        [HttpGet]
        public IActionResult CreateWithSelect(string? name, Guid? gameId, string? password, bool isPrivate = false)
        {
            // Récupérer tous les jeux pour la liste déroulante
            var games = _gameService.GetAvailableGames();
            ViewBag.Games = games;
            ViewBag.GameName = name;
            ViewBag.GameId = gameId;
            ViewBag.IsPrivate = isPrivate;
            ViewBag.Password = password;
            return View();
        }

        // Crée un lobby à partir du formulaire CreateWithSelect
        [HttpPost]
        public async Task<IActionResult> CreateWithSelect(string name, Guid gameId, bool isPrivate, string? password)
        {
            int? playerId = null;
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                playerId = player.Id;
            }
            else
            {
                //Gérer la redirection pour obtenir l'utilisateur
                var returnUrl = Url.Action("CreateWithSelect", "Lobby",
                new { name, gameId, isPrivate, password }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }
            try
            {
                var lobby = _lobbyService.CreateLobby(name, gameId, isPrivate, playerId, password);
                return RedirectToAction("Details", new { id = lobby.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Games = _gameService.GetAvailableGames();
                return View();
            }
        }

        // Crée un lobby à partir d'un jeu (le code du jeu est passé en paramètre)
        [HttpGet]
        public IActionResult CreateFromGame(string gameCode, string name, bool isPrivate, string? password)
        {
            var game = _gameService.GetGameByCode(gameCode);
            if (game == null)
            {
                return NotFound("Le jeu spécifié n'existe pas.");
            }

            ViewBag.GameName = name;
            ViewBag.IsPrivate = isPrivate;
            ViewBag.Password = password;
            return View(game);
        }

        // Traite la création d'un lobby à partir d'un jeu
        [HttpPost]
        public async Task<IActionResult> CreateFromGame(string name, Guid gameId, bool isPrivate, string? password)
        {
            int? playerId = null;
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                playerId = player.Id;
            }
            else
            {
                //Gérer la redirection pour obtenir l'utilisateur
                String? gameCode = _gameService.GetGameById(gameId)?.Code;
                var returnUrl = Url.Action("CreateFromGame", "Lobby",
                new { gameCode, name, isPrivate, password }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }
            try
            {
                var lobby = _lobbyService.CreateLobby(name, gameId, isPrivate, playerId ,password);
                return RedirectToAction("Details", new { id = lobby.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                var game = _gameService.GetGameById(gameId);
                if (game == null)
                {
                    return NotFound("Le jeu spécifié n'existe pas.");
                }

                return View(game);
            }
        }

         [HttpPost("Lobby/Start/{id}")]
        public IActionResult Start(Guid id)
        {
            var result = _lobbyService.StartGame(id);
            return result;
        }

        public async Task<Player> GetCurrentPlayer()
        {
            // Récupérer l'ID du joueur depuis la session
            var playerId = HttpContext.Session.GetInt32("PlayerId");

            if (playerId.HasValue)
            {
                return await _playerService.GetPlayerByIdAsync(playerId.Value);
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> JoinPrivateLobby(Guid id, string password)
        {
            var lobby = _lobbyService.GetLobbyById(id);

            if (lobby == null)
            {
                return NotFound();
            }

            // Vérifier si le mot de passe correspond pour un lobby privé
            if (lobby.IsPrivate && lobby.Password != password)
            {
                return Unauthorized();
            }

            // Logique pour ajouter un joueur au lobby
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                try
                {
                    // Ajouter le joueur au lobby
                    _lobbyService.AddPlayerToLobby(id, player.Id);
                }
                catch (PlayerAlreadyInLobbyException ex)
                {
                    // Gérer l'exception spécifique de joueur déjà dans le lobby
                    return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
                }
                catch (Exception ex)
                {
                    // Gérer toute autre exception
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {

                //Gérer la redirection pour obtenir l'utilisateur
                var returnUrl = Url.Action("JoinPrivateLobby", "Lobby",
                new { id, password }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }

            return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
        }

        [HttpGet]
        public async Task<IActionResult> JoinPublicLobby(Guid id)
        {
            var lobby = _lobbyService.GetLobbyById(id);

            if (lobby == null)
            {
                return NotFound();
            }

            // Logique pour ajouter un joueur au lobby
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                try
                {
                    // Ajouter le joueur au lobby
                    _lobbyService.AddPlayerToLobby(id, player.Id);
                }
                catch (PlayerAlreadyInLobbyException ex)
                {
                    // Gérer l'exception spécifique de joueur déjà dans le lobby
                    return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
                }
                catch (Exception ex)
                {
                    // Gérer toute autre exception
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                //Gérer la redirection pour obtenir l'utilisateur
                var returnUrl = Url.Action("JoinLobby", "Lobby", new { id }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }

            return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
        }

        [HttpGet]
        public async Task<IActionResult> JoinLobby(Guid id)
        {
            var lobby = _lobbyService.GetLobbyById(id);

            if (lobby == null)
            {
                return NotFound();
            }

            // Logique pour ajouter un joueur au lobby
            var player = await GetCurrentPlayer();
            if (player != null)
            {
                try
                {
                    // Ajouter le joueur au lobby
                    _lobbyService.AddPlayerToLobby(id, player.Id);
                }
                catch (PlayerAlreadyInLobbyException ex)
                {
                    // Gérer l'exception spécifique de joueur déjà dans le lobby
                    return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
                }
                catch (Exception ex)
                {
                    // Gérer toute autre exception
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                //Gérer la redirection pour obtenir l'utilisateur
                var returnUrl = Url.Action("JoinLobby", "Lobby", new { id }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }

            return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ResumeLobby(Guid id)
        {
            var lobby = _lobbyService.GetLobbyById(id);

            if (lobby == null)
            {
                return NotFound();
            }

            // Logique pour ajouter un joueur au lobby
            var player = await GetCurrentPlayer();
            if (player == null)
            {
                //Gérer la redirection pour obtenir l'utilisateur
                var returnUrl = Url.Action("ResumeLobby", "Lobby", new { id }, Request.Scheme);
                return RedirectToAction("Player", "Home", new { returnUrl });
            }

            return RedirectToAction("Details", "Lobby", new { id = lobby.Id });
        }
    }
}