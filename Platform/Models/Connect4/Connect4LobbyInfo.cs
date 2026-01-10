namespace GamingPlatform.Models.Connect4;

public class Connect4LobbyInfo
{
    public string LobbyId { get; init; } = default!;
    public string? HostConnectionId { get; set; }
    public bool Started { get; set; }

    public List<Connect4LobbyPlayer> Players { get; } = new();
}
