using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
namespace GamingPlatform.Models
{
public class Player
{
    public int Id { get; set; }    
    public string Pseudo { get; set; }

    public string? Name { get; set; }
    public string? email { get; set; }

    // Relation : Un joueur peut participer à plusieurs lobbies
    public ICollection<LobbyPlayer> LobbyPlayers { get; set; } = new List<LobbyPlayer>();
    }
}

