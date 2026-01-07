namespace GamingPlatform.Models.Connect4;

public enum Cell { Empty = 0, Red = 1, Yellow = 2 }
public enum GameStatus { WaitingPlayers, InProgress, Finished }

public class Connect4State
{
    public string LobbyId { get; }
    public string? RedId { get; set; }
    public string? YellowId { get; set; }

    // Board 6x7 (row 0 = top)
    public Cell[,] Board { get; } = new Cell[6, 7];
    public Cell Turn { get; set; } = Cell.Red;
    public GameStatus Status { get; set; } = GameStatus.WaitingPlayers;

    public Cell? Winner { get; set; }
    public bool IsDraw { get; set; }
    public int MoveCount { get; set; }

    public Connect4State(string lobbyId)
    {
    LobbyId = lobbyId;
    Reset();
    }


    public void Reset()
    {
    for (int r = 0; r < 6; r++)
        for (int c = 0; c < 7; c++)
            Board[r, c] = Cell.Empty;

    Turn = Cell.Red;
    Status = GameStatus.WaitingPlayers;
    Winner = null;
    IsDraw = false;
    MoveCount = 0;
    }   


}
