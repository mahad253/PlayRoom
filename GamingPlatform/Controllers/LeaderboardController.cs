using GamingPlatform.Models;
using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;


namespace GamingPlatform.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GameService _gameService;
        private readonly LobbyService _lobbyService;
        private readonly PlayerService _playerService;

        public LeaderboardController(LobbyService lobbyService, ILogger<HomeController> logger, GameService gameService, PlayerService playerService)
        {
            _logger = logger;
            _gameService = gameService;
            _playerService = playerService;
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
        }

        public IActionResult Index()
        {
            var topScoresPerGame = _lobbyService.GetTopScoresPerGame();
            return View(topScoresPerGame);
        }

    }
}
