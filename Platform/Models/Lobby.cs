namespace GamingPlatform.Models;

public class Lobby
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GameType { get; set; } = "Morpion";

    public int MaxPlayers { get; set; } = 2;

    public LobbyStatus Status { get; set; } = LobbyStatus.Waiting;

    public string? HostConnectionId { get; set; }

    public List<LobbyPlayer> Players { get; set; } = new();

    // État du jeu associé (pour Morpion uniquement ici)
    public Morpion Game { get; set; } = new Morpion();
}
