using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GamingPlatform.Hubs;
using Microsoft.Data.SqlClient;

namespace GamingPlatform.Services
{
    public class LobbyService
    {
        private readonly GamingPlatformContext _context;

        private readonly IServiceProvider _serviceProvider;
        public LobbyService(GamingPlatformContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        // Crée un nouveau lobby.
        public Lobby CreateLobby(string name, Guid gameId, bool isPrivate, int? playerId = null, string? password = null)
        {
            var game = _context.Game.Find(gameId);
            if (game == null)
            {
                throw new Exception("Le jeu spécifié n'existe pas.");
            }

            var lobby = new Lobby
            {
                Id = Guid.NewGuid(),
                Name = name,
                GameCode = game.Code,
                Game = game,
                IsPrivate = isPrivate,
                Password = isPrivate ? password : null,
                Status = LobbyStatus.Waiting,
                CreatedAt = DateTime.Now
            };

            // Vérifier si un joueur est connecté
            if (playerId.HasValue)
            {
                Player? player = _context.Player.Find(playerId.Value);
                if (player != null)
                {
                    var lobbyPlayer = new LobbyPlayer
                    {
                        LobbyId = lobby.Id,
                        PlayerId = player.Id
                    };

                    lobby.LobbyPlayers.Add(lobbyPlayer);
                }
            }

            _context.Lobby.Add(lobby);
            _context.SaveChanges();

            return lobby;
        }

        // Met à jour le lobby
        public void UpdateLobby(Lobby lobby)
        {
            _context.Lobby.Update(lobby); // Marque le lobby comme modifié
            _context.SaveChanges(); // Enregistre les modifications dans la base
        }

        // Récupère un lobby par son ID.
        public Lobby? GetLobbyById(Guid id)
        {
            return _context.Lobby
                .Where(l => l.Id == id)
                .FirstOrDefault();
        }

        // Méthode pour récupérer un lobby et charger les joueurs associés
        public Lobby GetLobbyWithGameAndPlayers(Guid lobbyId)
        {
            var lobby = _context.Lobby
                .Where(l => l.Id == lobbyId)
                .Include(l => l.Game)
                .Include(l => l.LobbyPlayers)  // Inclure les joueurs du lobby
                .ThenInclude(lp => lp.Player)  // Inclure les informations sur le joueur
                .FirstOrDefault();

            return lobby;  // Retourne le lobby avec les joueurs associés
        }


        // Récupère tous les lobbies disponibles.
        public async Task<IEnumerable<Lobby>> GetAllLobbies()
        {
            return await _context.Lobby
                .Include(l => l.Game) // Inclut l'objet Game lié au Lobby
                .Include(l => l.LobbyPlayers) // Inclut la collection LobbyPlayers
                    .ThenInclude(lp => lp.Player) // Inclut les objets Player dans LobbyPlayers
                .ToListAsync();
        }

        public async Task<List<Lobby>> GetLobbiesByGameAsync(string gameCode)
        {
            return await _context.Lobby
                .Include(l => l.Game) // Inclure les détails du jeu
                .Include(l => l.LobbyPlayers) // Inclure les joueurs du lobby
                    .ThenInclude(lp => lp.Player) // Inclure les détails des joueurs
                .Where(l => l.GameCode == gameCode)
                .ToListAsync();
        }

        // Ajoute un joueur à un lobby.
        public void AddPlayerToLobby(Guid lobbyId, int playerId)
        {
            var lobby = _context.Lobby.Find(lobbyId);
            var player = _context.Player.Find(playerId);

            if (lobby == null || player == null)
            {
                throw new Exception("Le lobby ou le joueur n'existe pas.");
            }

            if (lobby.LobbyPlayers.Any(lp => lp.PlayerId == playerId))
            {
                throw new PlayerAlreadyInLobbyException("Le joueur est déjà dans le lobby.");
            }

            var lobbyPlayer = new LobbyPlayer
            {
                LobbyId = lobbyId,
                PlayerId = playerId
            };

            _context.LobbyPlayer.Add(lobbyPlayer);
            _context.SaveChanges();
        }

        //
        public IActionResult StartGame(Guid lobbyId)
        {
            try
            {
                var lobby = GetLobbyWithGameAndPlayers(lobbyId);

                if (lobby == null)
                {
                    return new NotFoundObjectResult(new { Message = "Lobby introuvable" });
                }

                if (lobby.Status != LobbyStatus.Waiting)
                {

                    return new BadRequestObjectResult(new { Message = "Le lobby n'est pas en attente. il est en: {lobby.Status}" });
                }

                lobby.Status = LobbyStatus.InProgress;
                UpdateLobby(lobby);

                switch (lobby.Code)
                {
                    case "SPT":
                        {
                            var hubContext = _serviceProvider.GetRequiredService<IHubContext<SpeedTypingHub>>();
                            var speedTypingGame = new SpeedTyping(hubContext);
                            speedTypingGame.InitializeBoard();

                            lobby.Game.Name = "SpeedTyping";
                            UpdateLobby(lobby);

                            hubContext.Clients.Group(lobbyId.ToString()).SendAsync("GameStarted", speedTypingGame.TextToType, speedTypingGame.TimeLimit);

                            return new OkObjectResult(new
                            {
                                Message = "Partie de Speed Typing démarrée",
                                RedirectUrl = $"/Game/SpeedTyping/Play/{lobbyId}"
                            });
                        }

                    case "MOR":
                        {
                            var hubContext = _serviceProvider.GetRequiredService<IHubContext<MorpionHub>>();
                            var morpion = new Morpion();
                            morpion.InitializeBoard();

                            // Notifie les joueurs via SignalR
                            hubContext.Clients.Group(lobbyId.ToString()).SendAsync("GameStarted", morpion.RenderBoard());
                            return new OkObjectResult(new
                            {
                                Message = "Partie de Morpion démarrée",
                                RedirectUrl = $"/Game/Morpion/Play/{lobbyId}"
                            });
                        }
                    case "LAB":
                        {
                            var players = lobby.LobbyPlayers.ToList();
                            if (players.Count < 2)
                            {
                                return new ObjectResult(new { Message = $"Une erreur est survenue: on a pas deux joueurs dans le jeu" });
                            }
                            UpdateLobby(lobby);

                            return new OkObjectResult(new
                            {
                                RedirectUrl = $"/Game/Labyrinth/Play/{lobbyId}",
                                Message = "Partie de Course de labyrinthe démarrée"
                            });
                        }

                    case "PTB":
                        {
                            return new OkObjectResult(new
                            {
                                Message = "Partie de Petit Bac démarrée",
                                RedirectUrl = $"/PetitBac/Configure?lobbyId={lobbyId}" // Remplacez ceci par la logique appropriée si nécessaire.
                            });
                        }

                    default:
                        return new BadRequestObjectResult(new { Message = $"Le type de jeu avec le code {lobby.Code} n'est pas pris en charge." });
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { Message = $"Une erreur est survenue : {ex.Message}" }) { StatusCode = 500 };
            }
        }


        public List<IGrouping<string?, Score>> GetTopScoresPerGame(int topCount = 5)
        {
            var scoresWithGameType = _context.Score
                .Join(_context.Lobby,
                    score => score.LobbyId,
                    lobby => lobby.Id,
                    (score, lobby) => new { Score = score, GameType = lobby.GameType })
                .ToList();

            return scoresWithGameType
                .GroupBy(x => x.GameType)
                .Select(group => new
                {
                    GameType = group.Key,
                    TopScores = group
                        .OrderByDescending(x => x.Score.WPM)
                        .ThenByDescending(x => x.Score.Accuracy)
                        .Take(topCount)
                        .Select(x => x.Score)
                })
                .Where(x => x.TopScores.Any())
                .Select(x => x.TopScores.GroupBy(s => x.GameType).First())
                .ToList();
        }



    }
}
public class PlayerAlreadyInLobbyException : Exception
{
    public PlayerAlreadyInLobbyException() { }

    public PlayerAlreadyInLobbyException(string message) : base(message) { }

    public PlayerAlreadyInLobbyException(string message, Exception inner) : base(message, inner) { }
}

