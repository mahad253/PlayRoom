using GamingPlatform.Data;
using GamingPlatform.Models;
using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;


namespace GamingPlatform.Hubs
{
    public class SpeedTypingHub : Hub
    {
        private readonly SpeedTyping _game;
        private readonly GamingPlatformContext _context;
        private readonly LobbyService _lobbyService;
        private readonly ILogger<SpeedTypingHub> _logger;
        public readonly ConcurrentDictionary<string, string> _connectionToPseudo;

        public SpeedTypingHub(
            GamingPlatformContext context,
            SpeedTyping game,
            LobbyService lobbyService,
            ILogger<SpeedTypingHub> logger,
           ConcurrentDictionary<string, string> connectionToPseudo)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionToPseudo = connectionToPseudo ?? throw new ArgumentNullException(nameof(connectionToPseudo));
            game.SetConnectionToPseudo(_connectionToPseudo); // Passer le dictionnaire à SpeedTyping
            _logger.LogInformation("SpeedTypingHub instantiated successfully.");
            _context = context;
        }

        public async Task StartGame(string difficulty)
        {
            if (Enum.TryParse<Difficulty>(difficulty, true, out Difficulty parsedDifficulty))
            {
                try
                {
                    _logger.LogInformation("Starting game with difficulty: {Difficulty}", parsedDifficulty);
                    _game.InitializeBoard(parsedDifficulty);
                    _logger.LogInformation("Game initialized with text: {TextToType} and time limit: {TimeLimit}", _game.TextToType, _game.TimeLimit);
                    await Clients.All.SendAsync("GameStarted", _game.TextToType, _game.TimeLimit);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in StartGame for difficulty: {Difficulty}", parsedDifficulty);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Invalid difficulty received: {Difficulty}", difficulty);
                throw new ArgumentException("Invalid difficulty", nameof(difficulty));
            }
        }

        public async Task JoinLobby(string lobbyId, string pseudo)
        {
            if (string.IsNullOrWhiteSpace(lobbyId)) throw new ArgumentNullException(nameof(lobbyId));
            if (string.IsNullOrWhiteSpace(pseudo)) throw new ArgumentNullException(nameof(pseudo));

            try
            {
                _connectionToPseudo[Context.ConnectionId] = pseudo; // Associer le pseudo à l'identifiant de connexion

                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                Context.Items["LobbyId"] = lobbyId;

                _logger.LogInformation("Player '{Pseudo}' joined lobby '{LobbyId}'", pseudo, lobbyId);
                await Clients.Group(lobbyId).SendAsync("PlayerJoined", pseudo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding player '{Pseudo}' to lobby '{LobbyId}'", pseudo, lobbyId);
                throw;
            }
        }

        public async Task InitializeLobbyPlayers(string lobbyId)
        {
            if (string.IsNullOrWhiteSpace(lobbyId)) throw new ArgumentNullException(nameof(lobbyId));

            try
            {
                var lobby = _lobbyService.GetLobbyWithGameAndPlayers(Guid.Parse(lobbyId));
                if (lobby != null)
                {
                    foreach (var lobbyPlayer in lobby.LobbyPlayers)
                    {
                        if (lobbyPlayer.Player != null)
                        {
                            _connectionToPseudo[Context.ConnectionId] = lobbyPlayer.Player.Pseudo;
                            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                        }
                    }
                    _logger.LogInformation("Initialized players for lobby '{LobbyId}'", lobbyId);
                }
                else
                {
                    _logger.LogWarning("Lobby not found for ID: {LobbyId}", lobbyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing players for lobby '{LobbyId}'", lobbyId);
                throw;
            }
        }

        public async Task ChangeDifficulty(string difficulty)
        {
            if (Enum.TryParse<Difficulty>(difficulty, true, out Difficulty parsedDifficulty))
            {
                try
                {
                    var lobbyId = Context.Items["LobbyId"]?.ToString();
                    if (string.IsNullOrWhiteSpace(lobbyId))
                        throw new InvalidOperationException("Lobby ID not found in connection context.");

                    _logger.LogInformation("Changing difficulty to {Difficulty} for lobby {LobbyId}", parsedDifficulty, lobbyId);

                    // Diffuse la difficulté à tous les membres du lobby
                    await Clients.Group(lobbyId).SendAsync("DifficultyChanged", difficulty);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing difficulty to {Difficulty}", difficulty);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Invalid difficulty received: {Difficulty}", difficulty);
                throw new ArgumentException("Invalid difficulty", nameof(difficulty));
            }
        }


        public async Task UpdateProgress(string typedText)
        {
            try
            {
                await _game.CheckProgress(Context.ConnectionId, typedText);

                // Récupérer le pseudo associé à la connexion
                var pseudo = _connectionToPseudo.GetValueOrDefault(Context.ConnectionId, "Unknown");

                _logger.LogInformation("Progress updated for player '{Pseudo}' with connection '{ConnectionId}'", pseudo, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                var pseudo = _connectionToPseudo.GetValueOrDefault(Context.ConnectionId, "Unknown");
                _logger.LogError(ex, "Error updating progress for player '{Pseudo}' with connection '{ConnectionId}'", pseudo, Context.ConnectionId);
                throw;
            }
        }


        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

            try
            {
                string pseudo = _connectionToPseudo.GetValueOrDefault(Context.ConnectionId, "Unknown");
                await Clients.All.SendAsync("ReceiveMessage", pseudo, message);
                _logger.LogInformation("Message sent from '{Pseudo}': {Message}", pseudo, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from '{ConnectionId}'", Context.ConnectionId);
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                if (Context.ConnectionId == null)
                {
                    _logger.LogWarning("ConnectionId null detected.");
                    throw new InvalidOperationException("Client not connected.");
                }

                _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client connection: {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionToPseudo.TryRemove(Context.ConnectionId, out var pseudo))
            {
                _logger.LogInformation("Client déconnecté : {ConnectionId}, Pseudo : {Pseudo}", Context.ConnectionId, pseudo);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task BroadcastScores(Dictionary<string, int> scores)
        {
            var formattedScores = scores
            .Where(kvp => !double.IsInfinity(kvp.Value) && !double.IsNaN(kvp.Value))
            .ToDictionary(
                kvp => _connectionToPseudo.GetValueOrDefault(kvp.Key, "Unknown"),
                kvp => kvp.Value
            );
            foreach (var kvp in scores)
            {
                if (double.IsInfinity(kvp.Value) || double.IsNaN(kvp.Value))
                {
                    _logger.LogWarning("Invalid numeric value detected for key {Key}: {Value}", kvp.Key, kvp.Value);
                }
            }

            await Clients.All.SendAsync("ScoreUpdate", formattedScores);


        }
        public async Task EndGame(List<PlayerScore> leaderboard)
        {
            try
            {
                _logger.LogInformation("Game Over. Sending leaderboard to clients.");
                _logger.LogInformation("Current _connectionToPseudo mapping: {@_connectionToPseudo}", _connectionToPseudo);

                // Mapper les scores avec leurs pseudos
                var leaderboardWithPseudos = leaderboard.Select(playerScore => new
                {
                    playerId = playerScore.PlayerId,
                    pseudo = _connectionToPseudo.TryGetValue(playerScore.PlayerId, out var pseudo) ? pseudo : "Unknown", // Vérifiez ici
                    wpm = playerScore.WPM,
                    accuracy = playerScore.Accuracy,
                    score = playerScore.Score
                }).ToList();

                _logger.LogInformation("Leaderboard with pseudos: {@leaderboardWithPseudos}", leaderboardWithPseudos);

                await Clients.All.SendAsync("GameOver", leaderboardWithPseudos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending leaderboard.");
                throw;
            }
        }
        public async Task SaveScores(List<Score> scores, Guid lobbyId)
        {
            _logger.LogInformation("Received scores: {@Scores}, LobbyId: {LobbyId}", scores, lobbyId);

            if (scores == null || scores.Any(score => score == null))
            {
                _logger.LogError("Invalid scores data received");
                throw new InvalidDataException("One or more scores are null or improperly formatted.");
            }

            if (lobbyId == Guid.Empty)
            {
                _logger.LogError("Invalid lobbyId received: {LobbyId}", lobbyId);
                throw new ArgumentNullException(nameof(lobbyId));
            }

            try
            {
                _logger.LogInformation("Processing {ScoreCount} scores for lobby {LobbyId}", scores.Count, lobbyId);

                // Mapper les scores en ajoutant les détails nécessaires à la base de données
                var scoresToSave = scores.Select(score => new Score
                {
                    LobbyId = lobbyId,
                    PlayerId = score.PlayerId,
                    Pseudo = _connectionToPseudo.GetValueOrDefault(score.PlayerId, "Unknown"),
                    WPM = score.WPM,
                    Accuracy = score.Accuracy,
                    Difficulty = score.Difficulty,
                    DatePlayed = DateTime.UtcNow
                }).ToList();

                _logger.LogDebug("Mapped scores: {@ScoresToSave}", scoresToSave);

                // Sauvegarder les scores dans la base de données
                await _context.Score.AddRangeAsync(scoresToSave);
                var savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved {SavedCount} scores for lobby {LobbyId}", savedCount, lobbyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save scores for lobby {LobbyId}", lobbyId);
                throw;
            }
        }



    }
}
