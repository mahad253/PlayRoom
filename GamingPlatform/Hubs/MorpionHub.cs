using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamingPlatform.Hubs
{
    public class MorpionHub : Hub
    {
        private static readonly Dictionary<string, Lobby> Lobbies = new();

        public async Task JoinLobby(string lobbyId)
        {
            if (!Lobbies.ContainsKey(lobbyId))
            {
                Lobbies[lobbyId] = new Lobby();
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("PlayerJoined", lobbyId);
        }

        public async Task MakeMove(string lobbyId, int row, int col)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;

            var lobby = Lobbies[lobbyId];
            
            if (lobby.Board[row, col] != null || lobby.IsGameOver)
            {
                await Clients.Caller.SendAsync("InvalidMove", "Mouvement invalide.");
                return;
            }

            lobby.Board[row, col] = lobby.CurrentPlayer;

            await Clients.Group(lobbyId).SendAsync("ReceiveMove", row, col, lobby.CurrentPlayer);

            if (CheckForWin(lobby.Board, lobby.CurrentPlayer))
            {
                lobby.IsGameOver = true;
                lobby.Winner = lobby.CurrentPlayer;
                string winnerSymbol = lobby.CurrentPlayer;
                await Clients.Group(lobbyId).SendAsync("GameOver", winnerSymbol);
                return;

            }

            if (IsBoardFull(lobby.Board))
            {
                lobby.IsGameOver = true;
                await Clients.Group(lobbyId).SendAsync("GameOver", "null");
                return;
            }
            if (lobby.IsGameOver == false){

                lobby.CurrentPlayer = lobby.CurrentPlayer == "X" ? "O" : "X";
                await Clients.Group(lobbyId).SendAsync("UpdateCurrentPlayer", lobby.CurrentPlayer);
             }
        }

        public async Task ResetGame(string lobbyId)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;

            var lobby = Lobbies[lobbyId];
            lobby.Board = new string[3, 3];
            lobby.CurrentPlayer = "X";
            lobby.IsGameOver = false;
            lobby.Winner = null;

            await Clients.Group(lobbyId).SendAsync("ReceiveReset");
        }


        private bool CheckForWin(string[,] board, string player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) return true;
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player) return true;
            }

            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) return true;
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player) return true;

            return false;
        }

        private bool IsBoardFull(string[,] board)
        {
            foreach (var cell in board)
            {
                if (cell == null) return false;
            }
            return true;
        }

   
















 private class Lobby
        {
            public string[,] Board { get; set; } = new string[3, 3];
            public string CurrentPlayer { get; set; } = "X";
            public bool IsGameOver { get; set; } = false;
            public string Winner { get; set; }
            public string PlayerX { get; set; }
            public string PlayerO { get; set; }
        }
    }
}
