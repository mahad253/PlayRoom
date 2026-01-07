namespace GamingPlatform.Models.Connect4;

public class Connect4Dto
{
    public string LobbyId { get; set; } = default!;
    public int[][] Board { get; set; } = default!;
    public int Turn { get; set; }
    public string Status { get; set; } = default!;
    public int? Winner { get; set; }
    public bool IsDraw { get; set; }

    public bool HasRed { get; set; }
    public bool HasYellow { get; set; }
}
