using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Hubs
{
    public class MorpionHub : Hub
    {
        private readonly LobbyService _lobbyService;

        public MorpionHub(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        private static string GroupName(string lobbyId) => $"lobby-{lobbyId}";

        // =========================
        // JOIN LOBBY
        // =========================
        public async Task JoinLobby(string lobbyId, string pseudo)
        {
            var (lobby, player) = _lobbyService.JoinLobby(
                lobbyId,
                Context.ConnectionId,
                pseudo
            );

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lobbyId));

            // Message système
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
                canStart = lobby.Players.Count == lobby.MaxPlayers
                           && lobby.Status == Models.LobbyStatus.Waiting,
                hostConnectionId = lobby.HostConnectionId
            });
        }

        // =========================
        // START GAME (HOST ONLY)
        // =========================
        public async Task StartGame(string lobbyId)
        {
            var lobby = _lobbyService.StartGame(lobbyId, Context.ConnectionId);

            foreach (var p in lobby.Players)
            {
                await Clients.Client(p.ConnectionId)
                    .SendAsync("YouAre", new { symbol = p.Symbol });
            }

            await Clients.Group(GroupName(lobbyId)).SendAsync("ChatSystem", new
            {
                message = "🎮 La partie a commencé"
            });

            await Clients.Group(GroupName(lobbyId)).SendAsync("GameStarted", new
            {
                status = lobby.Status.ToString(),
                players = lobby.Players.Select(p => new
                {
                    pseudo = p.Pseudo,
                    isHost = p.IsHost
                }).ToList(),
                game = new
                {
                    board = lobby.Game.Board,
                    finished = lobby.Game.Finished,
                    winner = lobby.Game.Winner,
                    currentPlayer = lobby.Game.CurrentPlayer
                }
            });
        }

        // =========================
        // PLAY MOVE
        // =========================
        public async Task PlayMove(string lobbyId, int index)
        {
            var lobby = _lobbyService.PlayMove(
                lobbyId,
                Context.ConnectionId,
                index
            );

            await Clients.Group(GroupName(lobbyId)).SendAsync("UpdateGame", new
            {
                status = lobby.Status.ToString(),
                game = new
                {
                    board = lobby.Game.Board,
                    finished = lobby.Game.Finished,
                    winner = lobby.Game.Winner,
                    currentPlayer = lobby.Game.CurrentPlayer
                }
            });
        }

        // =========================
        // RESET GAME (HOST ONLY)
        // =========================
        public async Task ResetGame(string lobbyId)
        {
            var lobby = _lobbyService.ResetGame(
                lobbyId,
                Context.ConnectionId
            );

            await Clients.Group(GroupName(lobbyId)).SendAsync("ChatSystem", new
            {
                message = "🔄 La partie a été relancée"
            });

            await Clients.Group(GroupName(lobbyId)).SendAsync("UpdateGame", new
            {
                status = lobby.Status.ToString(),
                game = new
                {
                    board = lobby.Game.Board,
                    finished = lobby.Game.Finished,
                    winner = lobby.Game.Winner,
                    currentPlayer = lobby.Game.CurrentPlayer
                }
            });
        }

        // =========================
        // CHAT MESSAGE
        // =========================
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

        // =========================
        // DISCONNECT
        // =========================
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
                    canStart = lobby.Players.Count == lobby.MaxPlayers
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
}
