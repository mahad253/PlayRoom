namespace GamingPlatform.Models;

public class LobbyPlayer
{
    public string ConnectionId { get; set; } = default!;
    public string Pseudo { get; set; } = default!;
    public string? Symbol { get; set; } 
    public bool IsHost { get; set; }
}
