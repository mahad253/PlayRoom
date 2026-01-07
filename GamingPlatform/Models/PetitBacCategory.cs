namespace GamingPlatform.Models
{
public class PetitBacCategory
{
    public int Id { get; set; }
    public string Name { get; set; } // Nom de la catégorie
    public int PetitBacGameId { get; set; } // Clé étrangère
    public PetitBacGame PetitBacGame { get; set; } // Navigation
}
}
