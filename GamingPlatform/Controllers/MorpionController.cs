using Microsoft.AspNetCore.Mvc;
using GamingPlatform.Services;
using GamingPlatform.Models;
using System;
using System.Linq;

namespace GamingPlatform.Controllers
{
    [Route("Game/MOR")]
    public class MorpionController : Controller
    {
        private readonly LobbyService _lobbyService;
        private readonly ILogger<MorpionController> _logger;

        public MorpionController(LobbyService lobbyService, ILogger<MorpionController> logger)
        {
            _lobbyService = lobbyService;
            _logger = logger;
        }

        [HttpGet("Play/{lobbyId}")]
        public IActionResult Play(Guid lobbyId)
        {
            var lobby = _lobbyService.GetLobbyWithGameAndPlayers(lobbyId);

            if (lobby == null)
            {
                _logger.LogError($"Lobby ID {lobbyId} introuvable.");
                return NotFound("Lobby introuvable.");
            }

            var playerPseudos = lobby.LobbyPlayers.Select(p => p.Player.Pseudo).ToList();

            if (playerPseudos.Count < 2)
            {
                _logger.LogError($"Le lobby ID {lobbyId} n'a pas assez de joueurs pour commencer.");
                return BadRequest("Au moins deux joueurs sont nécessaires pour commencer.");
            }

            var morpion = new Morpion
            {
                LobbyId = lobbyId,
                CurrentPlayer = playerPseudos[0],
                PlayerX = playerPseudos[0],
                PlayerO = playerPseudos[1],
                CurrentPlayerName = playerPseudos[0]
            };

            ViewData["LobbyId"] = lobbyId.ToString();
            ViewData["PlayerX"] = morpion.PlayerX;
            ViewData["PlayerO"] = morpion.PlayerO;
            ViewData["CurrentPlayer"] = morpion.CurrentPlayer;

            _logger.LogInformation($"Lobby {lobbyId} chargé pour Morpion avec {morpion.PlayerX} (X) et {morpion.PlayerO} (O).");
            return View(morpion);
        }
    }
}
