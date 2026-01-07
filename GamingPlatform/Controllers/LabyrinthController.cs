using GamingPlatform.Hubs;
using GamingPlatform.Models;
using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Controllers
{
    [Route("Game/LAB")]
    public class LabyrinthController : Controller
    {
        private readonly LabyrinthGenerator _labyrinthGenerator;

		private readonly LabyrinthService _labyrinthService;
        private readonly LobbyService _lobbyService;
        private readonly PlayerService _playerService;

        public LabyrinthController(IHubContext<LabyrinthHub> hubContext, LabyrinthService labyrinthService, LobbyService lobbyService, PlayerService playerService) { 
			_labyrinthService = labyrinthService;
            _lobbyService = lobbyService;
            _playerService = playerService;
        }
       
        [HttpGet("Maze/{lobbyId}")]
        public async Task<IActionResult> Maze(Guid lobbyId)
        {

            var isGenerated = await _labyrinthService.IsLabyrinthGeneratedForLobbyAsync(lobbyId);
            if (!isGenerated)
            {
                return NotFound("Aucun labyrinthe n'a été généré pour ce lobby.");
            }
            var lobby = _lobbyService.GetLobbyWithGameAndPlayers(lobbyId);
            var players = lobby.LobbyPlayers.ToList();
            if (players.Count < 2)
            {
                return NotFound("Une erreur est survenue: on a pas deux joueurs dans le jeu");
            }

            // Extraire les pseudos des joueurs
            var player1 = players[0].Player.Pseudo;
            var player2 = players[1].Player.Pseudo;
            var player = await GetCurrentPlayer();

            ViewBag.lobbyId = lobbyId;
            ViewBag.player1 = player1;
            ViewBag.player2 = player2;
            ViewBag.currentplayer = player.Pseudo;
            return View();
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
                return NotFound("Lobby non trouvé.");
            }

            var currentPlayer = await GetCurrentPlayer();
            if (currentPlayer == null)
            {
                return RedirectToAction("Player", "Home");
            }

            var players = lobby.LobbyPlayers.ToList();
            if (players.Count < 2)
            {
                return NotFound("Une erreur est survenue: on a pas deux joueurs dans le jeu");
            }

            var isGenerated = await _labyrinthService.IsLabyrinthGeneratedForLobbyAsync(lobbyId);

            if (isGenerated)
            {
                return RedirectToAction("Maze", "Labyrinth", new {lobbyId});
            }
            // Extraire les pseudos des joueurs
            var player1 = players[0].Player.Pseudo;
            var player2 = players[1].Player.Pseudo;

            ViewBag.lobbyId = lobbyId;
            ViewBag.player1 = player1;
            ViewBag.player2 = player2;
            return View();
        }

        [HttpGet("getlabyrinth/{id}")]
        public async Task<IActionResult> GetLabyrinth(int id)
		{
			var labyrinth = await _labyrinthService.GetLabyrinthByIdAsync(id);
			if (labyrinth == null)
			{
				return NotFound(new { message = "Labyrinthe non trouvé." });
			}

			return Ok(new
			{
				id = labyrinth.Id,
				data = labyrinth.Data // Assurez-vous que Data contient une chaîne JSON valide
			});
		}

        [HttpGet("getlabyrinthlobby/{lobbyId}")]
        public async Task<IActionResult> GetLabyrinthLobby(Guid lobbyId)
        {
            var labyrinth = await _labyrinthService.GetLabyrinthByLobbyIdAsync(lobbyId);
            if (labyrinth == null)
            {
                return NotFound(new { message = "Labyrinthe non trouvé." });
            }

            return Ok(new
            {
                id = labyrinth.Id,
                data = labyrinth.Data 
            });
        }


    }

}
