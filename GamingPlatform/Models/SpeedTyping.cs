using GamingPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace GamingPlatform.Models
{

    public class SpeedTyping : IGameBoard
    {
        public string TextToType { get; private set; }
        public DateTime StartTime { get; private set; }
        public bool IsGameStarted { get; private set; }
        public int TimeLimit { get; private set; }
        public Difficulty CurrentDifficulty { get; private set; }

        private ConcurrentDictionary<string, string> _connectionToPseudo;

        //private readonly Dictionary<string, string> _connectionToPseudo = new Dictionary<string, string>();


        private readonly IHubContext<SpeedTypingHub> _hubContext;
        private Dictionary<string, PlayerStats> _playerStats = new Dictionary<string, PlayerStats>();

        public SpeedTyping(IHubContext<SpeedTypingHub> hubContext)
        {
            _hubContext = hubContext;
            TextToType = string.Empty;
        }

        public void SetConnectionToPseudo(ConcurrentDictionary<string, string> connectionToPseudo)
        {
            _connectionToPseudo = connectionToPseudo ?? throw new ArgumentNullException(nameof(connectionToPseudo));
        }

        public void InitializeBoard(Difficulty difficulty)
        {
            Console.WriteLine($"Début de InitializeBoard avec difficulté {difficulty}");
            try
            {
                Console.WriteLine($"Initializing board for difficulty: {difficulty}");
                CurrentDifficulty = difficulty;

                TextToType = GenerateRandomText(difficulty);
                if (string.IsNullOrEmpty(TextToType))
                {
                    throw new Exception("Failed to generate text for difficulty.");
                }

                Console.WriteLine($"Board initialized with text: {TextToType}");

                StartTime = DateTime.Now;
                IsGameStarted = true;

                TimeLimit = GetTimeLimit(difficulty);
                Console.WriteLine($"Time limit set to: {TimeLimit}");

                _playerStats.Clear();

                // Envoyer le message après toutes les initialisations
                _hubContext.Clients.All.SendAsync("GameStarted", TextToType, TimeLimit).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"Error sending GameStarted message: {task.Exception.InnerException?.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing board: {ex.Message}");
                //Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        public string RenderBoard()
        {
            return TextToType;
        }

        public bool IsGameOver()
        {
            return !IsGameStarted || (DateTime.Now - StartTime).TotalSeconds >= TimeLimit;
        }

        public async Task CheckProgress(string playerId, string typedText)
        {
            if (!_playerStats.ContainsKey(playerId))
            {
                _playerStats[playerId] = new PlayerStats(StartTime);
            }

            bool isCorrect = TextToType.StartsWith(typedText);
            int progress = (int)((double)typedText.Length / TextToType.Length * 100);

            _playerStats[playerId].UpdateStats(typedText, isCorrect);

            int points = CalculatePoints(_playerStats[playerId].WPM, _playerStats[playerId].Accuracy, CurrentDifficulty);

            await _hubContext.Clients.Client(playerId).SendAsync("ProgressUpdate", isCorrect, progress, _playerStats[playerId].WPM, _playerStats[playerId].Accuracy, points);

            if (typedText == TextToType || IsGameOver())
            {
                await EndGame();
            }
        }


        private async Task EndGame()
        {
            IsGameStarted = false;

            var leaderboard = _playerStats
            .OrderByDescending(p => p.Value.WPM)
            .Select(p => new
            {
                PlayerId = p.Key, // p.Key est déjà Context.ConnectionId
                Pseudo = _connectionToPseudo.TryGetValue(p.Key, out var pseudo) ? pseudo : "Unknown",
                WPM = p.Value.WPM,
                Accuracy = p.Value.Accuracy,
                Score = CalculatePoints(p.Value.WPM, p.Value.Accuracy, CurrentDifficulty)
            })
            .ToList();
            await _hubContext.Clients.All.SendAsync("GameOver", leaderboard);
        }


        private string GenerateRandomText(Difficulty difficulty)
        {
            string[] sentences = new string[]
            {
                "The quick brown fox jumps over the lazy dog.",
                "A journey of a thousand miles begins with a single step.",
                "To be or not to be, that is the question.",
                "All that glitters is not gold.",
                "Where there's a will, there's a way.",
                "Actions speak louder than words.",
                "Knowledge is power, guard it well.",
                "Practice makes perfect.",
                "Two wrongs don't make a right.",
                "When in Rome, do as the Romans do."
            };

            Random random = new Random();
            int sentenceCount = difficulty switch
            {
                Difficulty.Easy => random.Next(2, 4),
                Difficulty.Medium => random.Next(4, 6),
                Difficulty.Hard => random.Next(6, 9),
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty)),
            };

            List<string> selectedSentences = new List<string>();
            for (int i = 0; i < sentenceCount; i++)
            {
                int index = random.Next(0, sentences.Length);
                selectedSentences.Add(sentences[index]);
            }

            return string.Join(" ", selectedSentences);
        }

        private int GetTimeLimit(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => 60,
                Difficulty.Medium => 90,
                Difficulty.Hard => 120,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty)),
            };
        }

        private int CalculatePoints(int wpm, double accuracy, Difficulty difficulty)
        {
            int baseScore = wpm * 10;
            double accuracyMultiplier = accuracy / 100;
            int difficultyMultiplier = difficulty switch
            {
                Difficulty.Easy => 1,
                Difficulty.Medium => 2,
                Difficulty.Hard => 3,
                _ => 1
            };

            int rawScore = (int)(baseScore * accuracyMultiplier);
            int adjustedScore = rawScore * difficultyMultiplier;

            return adjustedScore;
        }


        public void InitializeBoard()
        {
            throw new NotImplementedException();
        }

        public class PlayerStats
        {
            private DateTime StartTime;
            public int TotalCharacters { get; private set; }
            public int CorrectCharacters { get; private set; }
            public int WPM { get; private set; }
            public double Accuracy { get; private set; }

            public PlayerStats(DateTime startTime)
            {
                StartTime = startTime;
            }

            public void UpdateStats(string typedText, bool isCorrect)
            {
                TotalCharacters = typedText.Length;
                CorrectCharacters = isCorrect ? typedText.Length : CorrectCharacters;
                WPM = (int)(CorrectCharacters / 5.0 / (DateTime.Now - StartTime).TotalMinutes);
                Accuracy = (double)CorrectCharacters / TotalCharacters * 100;
            }
        }
    }
}
