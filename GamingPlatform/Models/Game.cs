namespace GamingPlatform.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Code { get; set; } // Utilisé pour reconnaître le jeu à lancer
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
    }

}
