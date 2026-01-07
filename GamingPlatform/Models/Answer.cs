namespace GamingPlatform.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Category { get; set; } // Catégorie (clé du dictionnaire)
        public string Response { get; set; } // Réponse (valeur du dictionnaire)

        // Relation avec PetitBacPlayer
        public int PetitBacPlayerId { get; set; }
        public PetitBacPlayer PetitBacPlayer { get; set; }
    }
}
