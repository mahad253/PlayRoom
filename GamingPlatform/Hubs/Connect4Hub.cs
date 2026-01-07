using GamingPlatform.Models.Connect4;
using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Hubs;

public class Connect4Hub : Hub
{
    private readonly IGameStore _store;
    public Connect4Hub(IGameStore store) => _store = store;

    public async Task JoinLobby(string lobbyId)
    {


        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

        var game = _store.GetOrCreate(lobbyId);
          // Si personne n'a encore rejoint, on reset pour être sûr
        if (game.RedId is null && game.YellowId is null)
        {
            game.Reset();
        }

        if (game.RedId is null) game.RedId = Context.ConnectionId;
        else if (game.YellowId is null && game.RedId != Context.ConnectionId) game.YellowId = Context.ConnectionId;

        if (game.RedId != null && game.YellowId != null)
            game.Status = GameStatus.InProgress;

        await Clients.Group(lobbyId).SendAsync("GameStateUpdated", ToDto(game));
        Console.WriteLine($"[C4] JoinLobby lobby={lobbyId} status={game.Status} turn={game.Turn} moves={game.MoveCount} winner={game.Winner} draw={game.IsDraw} red={(game.RedId!=null)} yellow={(game.YellowId!=null)}");
    }

    public async Task PlayMove(string lobbyId, int col)
    {
        if (!_store.TryGet(lobbyId, out var game))
        {
            await Clients.Caller.SendAsync("Error", "Lobby introuvable");
            return;
        }

        if (game.Status != GameStatus.InProgress)
        {
            await Clients.Caller.SendAsync("Error", "Partie non démarrée / déjà finie");
            return;
        }

        var me = GetMyColor(game);
        if (me == Cell.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Vous n'êtes pas joueur dans ce lobby");
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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Option simple : on ne supprime pas tout de suite, mais on peut finir la partie
        // (tu peux améliorer après)
        await base.OnDisconnectedAsync(exception);
    }

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
}
