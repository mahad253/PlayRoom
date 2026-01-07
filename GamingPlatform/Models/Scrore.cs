using GamingPlatform.Models;
using System;

namespace GamingPlatform.Models
{
public class Score
{
    public Guid Id { get; set; } // Généré automatiquement si non fourni
    public Guid LobbyId { get; set; }
    public string PlayerId { get; set; }
    public string Pseudo { get; set; }
    public int WPM { get; set; }
    public float Accuracy { get; set; }
    public string Difficulty { get; set; }
    public DateTime DatePlayed { get; set; }
}

}