namespace GamingPlatform.Models.Connect4;

public class Connect4LobbyPlayer
{
    public string ConnectionId { get; set; } = default!;
    public string Pseudo { get; set; } = default!;
    public bool IsHost { get; set; }
    public int Color { get; set; } // 1 = Rouge, 2 = Jaune
}
