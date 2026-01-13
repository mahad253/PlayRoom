using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Hubs;

public class SpeedTypingHub : Hub
{
    private readonly LobbyService _lobbyService;

    public SpeedTypingHub(LobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }

    private static string GroupName(string lobbyId) => $"lobby-{lobbyId}";

    // JOIN LOBBY
    public async Task JoinLobby(string lobbyId, string pseudo)
    {
        var (lobby, player) = _lobbyService.JoinLobby(
            lobbyId,
            Context.ConnectionId,
            pseudo
        );

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lobbyId));

        await Clients.Group(GroupName(lobbyId)).SendAsync("ChatSystem", new
        {
            message = $"{pseudo} a rejoint le lobby"
        });

        await Clients.Group(GroupName(lobbyId)).SendAsync("LobbyUpdated", new
        {
            lobbyId = lobby.Id,
            status = lobby.Status.ToString(),
            players = lobby.Players.Select(p => new
            {
                pseudo = p.Pseudo,
                isHost = p.IsHost
            }).ToList(),
            canStart = lobby.Players.Count >= 2 && lobby.Status == Models.LobbyStatus.Waiting,
            hostConnectionId = lobby.HostConnectionId
        });
    }

    // START GAME (HOST ONLY)
    public async Task StartGame(string lobbyId, string difficulty)
    {
        string textToType = GenerateTextForDifficulty(difficulty);
        int durationSeconds = 60;

        var lobby = _lobbyService.StartSpeedTyping(
            lobbyId,
            Context.ConnectionId,
            textToType,
            durationSeconds
        );

        await Clients.Group(GroupName(lobbyId)).SendAsync("ChatSystem", new
        {
            message = "🎮 Partie SpeedTyping lancée"
        });

        await Clients.Group(GroupName(lobbyId)).SendAsync("GameStarted", new
        {
            status = lobby.Status.ToString(),
            textToType,
            duration = durationSeconds,
            players = lobby.SpeedTypingGame!.Players.Values.Select(p => new
            {
                pseudo = p.Pseudo,
                wpm = p.Wpm,
                accuracy = p.Accuracy,
                finished = p.Finished
            }).ToList()
        });
    }

    private string GenerateTextForDifficulty(string difficulty)
    {
        return difficulty switch
        {
            "easy" => "Les chats dorment souvent au soleil.",
            "medium" => "Les étudiants de M2 GIL développent une plateforme de jeux.",
            "hard" => "La synchronisation temps réel avec SignalR teste la précision de frappe.",
            _ => "Tapez ce texte pour démarrer la partie de SpeedTyping."
        };
    }

    // UPDATE PROGRESS
    public async Task UpdateProgress(string lobbyId, int totalTyped, int correctChars)
    {
        var lobby = _lobbyService.UpdateSpeedTypingProgress(
            lobbyId,
            Context.ConnectionId,
            totalTyped,
            correctChars
        );

        var game = lobby.SpeedTypingGame!;
        await Clients.Group(GroupName(lobbyId)).SendAsync("ProgressUpdated", new
        {
            status = lobby.Status.ToString(),
            players = game.Players.Values.Select(p => new
            {
                pseudo = p.Pseudo,
                wpm = p.Wpm,
                accuracy = p.Accuracy,
                finished = p.Finished,
                correctChars = p.CorrectChars,
                totalTyped = p.TotalTyped
            }).ToList()
        });
    }

    // CHAT MESSAGE
    public async Task SendChatMessage(string lobbyId, string pseudo, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        await Clients.Group(GroupName(lobbyId)).SendAsync("ChatMessage", new
        {
            pseudo,
            message
        });
    }

    // DISCONNECT
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var lobby = _lobbyService.RemovePlayer(Context.ConnectionId);

            await Clients.Group(GroupName(lobby.Id)).SendAsync("ChatSystem", new
            {
                message = "❌ Un joueur a quitté le lobby"
            });

            await Clients.Group(GroupName(lobby.Id)).SendAsync("LobbyUpdated", new
            {
                lobbyId = lobby.Id,
                status = lobby.Status.ToString(),
                players = lobby.Players.Select(p => new
                {
                    pseudo = p.Pseudo,
                    isHost = p.IsHost
                }).ToList(),
                canStart = lobby.Players.Count >= 2
                           && lobby.Status == Models.LobbyStatus.Waiting,
                hostConnectionId = lobby.HostConnectionId
            });
        }
        catch
        {
            // lobby déjà supprimé
        }

        await base.OnDisconnectedAsync(exception);
    }
}
