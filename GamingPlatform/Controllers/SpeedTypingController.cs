using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using GamingPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using GamingPlatform.Data;

namespace GamingPlatform.Controllers
{
    [Route("Game/SPT")]
    public class SpeedTypingController : Controller
    {
        private readonly LobbyService _lobbyService;
        private readonly PlayerService _playerService;
        private readonly GamingPlatformContext _context;
        private readonly ILogger<SpeedTypingController> _logger;

        public SpeedTypingController(GamingPlatformContext context, LobbyService lobbyService, ILogger<SpeedTypingController> logger, PlayerService playerService)
        {
            _lobbyService = lobbyService;
            _logger = logger;
            _playerService = playerService;
            _context = context;
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

        [HttpGet("Play/{lobbyId}")]
        public async Task<IActionResult> Play(Guid lobbyId)
        {
            var lobby = _lobbyService.GetLobbyWithGameAndPlayers(lobbyId);
            if (lobby == null)
            {
                _logger.LogError($"Lobby ID {lobbyId} not found.");
                return NotFound("Lobby not found.");
            }

            var currentPlayer = await GetCurrentPlayer();
            if (currentPlayer == null)
            {
                return RedirectToAction("Player", "Home");
            }

            var model = new SpeedTypingViewModel
            {
                LobbyId = lobby.Id,
                Name = lobby.Name,
                PlayerPseudos = lobby.LobbyPlayers.Select(lp => lp.Player.Pseudo).ToList(),
                CurrentPlayerPseudo = currentPlayer.Pseudo
            };
            return View(model);
        }



        [HttpPost("SaveScores")]
        public async Task<IActionResult> SaveScores([FromBody] SaveScoresRequest request)
        {
            var scores = request.Scores;
            var lobbyId = request.LobbyId;

            _logger.LogInformation("Received scores: {@Scores}, LobbyId: {LobbyId}", scores, lobbyId);

            if (scores == null || scores.Any(score => score == null))
            {
                _logger.LogError("Invalid scores data received");
                return BadRequest("One or more scores are null or improperly formatted.");
            }

            if (lobbyId == Guid.Empty)
            {
                _logger.LogError("Invalid lobbyId received: {LobbyId}", lobbyId);
                return BadRequest("Lobby ID cannot be empty.");
            }

            try
            {
                // Mapper les scores en ajoutant les détails nécessaires à la base de données
                var scoresToSave = scores.Select(score => new Score
                {
                    LobbyId = lobbyId,
                    PlayerId = score.PlayerId,
                    Pseudo = score.Pseudo,
                    WPM = score.WPM,
                    Accuracy = score.Accuracy,
                    Difficulty = score.Difficulty,
                    DatePlayed = DateTime.UtcNow
                }).ToList();

                // Sauvegarder les scores dans la base de données
                await _context.Score.AddRangeAsync(scoresToSave);
                var savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved {SavedCount} scores for lobby {LobbyId}", savedCount, lobbyId);
                return Ok(savedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save scores for lobby {LobbyId}", lobbyId);
                return StatusCode(500, "Internal server error");
            }
        }

        // Classe pour encapsuler les données reçues
        public class SaveScoresRequest
        {
            public List<Score> Scores { get; set; }
            public Guid LobbyId { get; set; }
        }


    }

}
