using System.Text.Json.Serialization;

namespace GamingPlatform.Models;

public class LobbyPlayer
{
    public int Id { get; set; }
    public string Nickname { get; set; } = null!;
    public bool IsHost { get; set; }

    public int LobbyId { get; set; }
    [JsonIgnore]
    public Lobby Lobby { get; set; } = null!;
}