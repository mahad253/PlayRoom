using GamingPlatform.Models.Connect4;
using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Hubs;

public class Connect4Hub : Hub
{
    private readonly IGameStore _store;

    public Connect4Hub(IGameStore store)
    {
        _store = store;
    }

    // ======================
    // JOIN LOBBY
    // ======================
    public async Task JoinLobby(string lobbyId, string pseudo)
    {
        var game = _store.GetOrCreate(lobbyId);
        var lobby = _store.GetLobbyInfo(lobbyId);

        // Ajouter le joueur s'il n'existe pas déjà
        if (!lobby.Players.Any(p => p.ConnectionId == Context.ConnectionId))
        {
            lobby.Players.Add(new Connect4LobbyPlayer
            {
                ConnectionId = Context.ConnectionId,
                Pseudo = pseudo,
                IsHost = false,
                Color = 0
            });


            // Premier joueur = host
            if (lobby.Players.Count == 1)
            {
                lobby.HostConnectionId = Context.ConnectionId;
                lobby.Players[0].IsHost = true;
            }

        }

        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

        await Clients.Group(lobbyId).SendAsync("LobbyUpdated", new
        {
            players = lobby.Players.Select(p => new
            {
                pseudo = p.Pseudo,
                isHost = p.ConnectionId == lobby.HostConnectionId
            }),
            canStart = lobby.Players.Count == 2 && !lobby.Started
        });
    }

    // ======================
    // START GAME (HOST ONLY)
    // ======================
    public async Task StartGame(string lobbyId)
    {
        var game = _store.GetOrCreate(lobbyId);
        var lobby = _store.GetLobbyInfo(lobbyId);

        if (Context.ConnectionId != lobby.HostConnectionId)
            throw new HubException("Seul le host peut démarrer la partie.");

        if (lobby.Players.Count != 2)
            throw new HubException("Il faut exactement 2 joueurs.");

        lobby.Started = true;

        // Assignation couleurs
        game.Reset();
        game.RedId = lobby.Players[0].ConnectionId;
        game.YellowId = lobby.Players[1].ConnectionId;
        game.Status = GameStatus.InProgress;

        await Clients.Group(lobbyId).SendAsync("ChatSystem", new
        {
            message = "🎮 La partie commence !"
        });

        await Clients.Group(lobbyId).SendAsync("GameStateUpdated", ToDto(game));
    }

    // ======================
    // PLAY MOVE
    // ======================
    public async Task PlayMove(string lobbyId, int col)
    {
        if (!_store.TryGet(lobbyId, out var game))
        {
            await Clients.Caller.SendAsync("Error", "Lobby introuvable");
            return;
        }

        if (game.Status != GameStatus.InProgress)
        {
            await Clients.Caller.SendAsync("Error", "La partie n'a pas commencé");
            return;
        }

        var me = GetMyColor(game);
        if (me == Cell.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Vous n'êtes pas joueur");
            return;
        }

        if (game.Turn != me)
        {
            await Clients.Caller.SendAsync("Error", "Ce n'est pas votre tour");
            return;
        }

        if (col < 0 || col > 6)
        {
            await Clients.Caller.SendAsync("Error", "Colonne invalide");
            return;
        }

        int row = Connect4Logic.FindDropRow(game.Board, col);
        if (row == -1)
        {
            await Clients.Caller.SendAsync("Error", "Colonne pleine");
            return;
        }

        game.Board[row, col] = me;
        game.MoveCount++;

        if (Connect4Logic.HasWon(game.Board, row, col, me))
        {
            game.Status = GameStatus.Finished;
            game.Winner = me;
        }
        else if (game.MoveCount >= 42)
        {
            game.Status = GameStatus.Finished;
            game.IsDraw = true;
        }
        else
        {
            game.Turn = (game.Turn == Cell.Red) ? Cell.Yellow : Cell.Red;
        }

        await Clients.Group(lobbyId).SendAsync("GameStateUpdated", ToDto(game));
    }

    // ======================
    // CHAT
    // ======================
    public async Task SendChatMessage(string lobbyId, string pseudo, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        await Clients.Group(lobbyId).SendAsync("ChatMessage", new
        {
            pseudo,
            message
        });
    }

    // ======================
    // DISCONNECT
    // ======================
   public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_store.TryRemovePlayer(Context.ConnectionId, out var lobbyId, out var lobby))
        {
            await Clients.Group(lobbyId).SendAsync("ChatSystem", new
            {
                message = "❌ Un joueur a quitté le lobby"
            });

            await Clients.Group(lobbyId).SendAsync("LobbyUpdated", new
            {
                players = lobby.Players.Select(p => new
                {
                    pseudo = p.Pseudo,
                    isHost = p.IsHost
                }),
                canStart = lobby.Players.Count == 2 && !lobby.Started
            });
        }

        await base.OnDisconnectedAsync(exception);
    }


    // ======================
    // HELPERS
    // ======================
    private Cell GetMyColor(Connect4State g)
    {
        if (g.RedId == Context.ConnectionId) return Cell.Red;
        if (g.YellowId == Context.ConnectionId) return Cell.Yellow;
        return Cell.Empty;
    }

    private static Connect4Dto ToDto(Connect4State g)
    {
        var board = new int[6][];
        for (int r = 0; r < 6; r++)
        {
            board[r] = new int[7];
            for (int c = 0; c < 7; c++)
                board[r][c] = (int)g.Board[r, c];
        }

        return new Connect4Dto
        {
            LobbyId = g.LobbyId,
            Board = board,
            Turn = (int)g.Turn,
            Status = g.Status.ToString(),
            Winner = g.Winner is null ? null : (int)g.Winner.Value,
            IsDraw = g.IsDraw,
            HasRed = g.RedId != null,
            HasYellow = g.YellowId != null
        };
    }

public async Task RestartGame(string lobbyId)
{
    // 1) récupérer lobby + game
    var lobby = _store.GetLobbyInfo(lobbyId);

    if (!_store.TryGet(lobbyId, out var game))
        game = _store.GetOrCreate(lobbyId);

    // 2) autorisation : seulement le host (celui qui a IsHost=true dans lobby)
    var host = lobby.Players.FirstOrDefault(p => p.IsHost);
    if (host == null || host.ConnectionId != Context.ConnectionId)
    {
        await Clients.Caller.SendAsync("Error", "Seul le host peut relancer.");
        return;
    }

    // 3) reset jeu (plateau + winner etc.)
    game.Reset();

    // 4) IMPORTANT : permettre de redémarrer
    lobby.Started = false;

    // 5) si 2 joueurs sont encore là, on reste en "WaitingPlayers" mais canStart = true
    if (game.RedId != null && game.YellowId != null)
        game.Status = GameStatus.WaitingPlayers;

    // 6) notifier lobby + état de jeu
    await Clients.Group(lobbyId).SendAsync("LobbyUpdated", new
    {
        players = lobby.Players.Select(p => new { pseudo = p.Pseudo, isHost = p.IsHost }),
        canStart = lobby.Players.Count == 2 && !lobby.Started
    });

    await Clients.Group(lobbyId).SendAsync("GameStateUpdated", ToDto(game));
}


}
