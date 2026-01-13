using System.ComponentModel.DataAnnotations;

namespace GamingPlatform.Models;

public class Player
{
    [Required]
    public string Pseudo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Nom { get; set; }
}
