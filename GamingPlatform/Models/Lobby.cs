namespace GamingPlatform.Models;

public class Lobby
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;   // code court à mettre dans l’URL
    public string GameType { get; set; } = "SpeedTyping";
    public string HostName { get; set; } = null!;
    public bool IsPrivate { get; set; }
    public string? Password { get; set; }       // pour ton TP tu peux laisser en clair
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Waiting"; // Waiting / Running / Finished

    public ICollection<LobbyPlayer> Players { get; set; } = new List<LobbyPlayer>();
}