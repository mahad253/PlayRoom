namespace GamingPlatform.Models;

public class LobbyPlayer
{
    public string ConnectionId { get; set; } = default!;
    public string Pseudo { get; set; } = default!;
    public string? Symbol { get; set; } // "X" ou "O" (quand la partie démarre)
    public bool IsHost { get; set; }
}
