namespace GamingPlatform.Models
{
public class Lobby
{
    public Guid Id { get; set; }
    public string Code { get; set; } = Guid.NewGuid().ToString(); // Code unique pour inviter les joueurs
    public string Name { get; set; }
    public string? GameType { get; set; }
    public bool IsPrivate { get; set; }
    public string? Password { get; set; } // Mot de passe pour les lobbies privés
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public LobbyStatus Status { get; set; }

    // Relation : Un lobby est lié à un jeu via le Code
    public string GameCode { get; set; }
    public Game Game { get; set; }

        // Relation : Un lobby contient plusieurs joueurs
    public ICollection<LobbyPlayer> LobbyPlayers { get; set; } = new List<LobbyPlayer>();

    // Relation avec PetitBacGame
        public PetitBacGame? PetitBacGame { get; set; } // Une partie de Petit Bac dans ce lobby

    }


public enum LobbyStatus
{
    Waiting,
    InProgress,
    Finished
}

}

