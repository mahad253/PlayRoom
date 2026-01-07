namespace GamingPlatform.Models
{
    public class SubmitScoreRequest
    {
        public string PlayerPseudo { get; set; }       // Correspond à `Id` dans PetitBacPlayer
        public int GameId { get; set; }         // Correspond à `PetitBacGameId` dans PetitBacPlayer
        public double Score { get; set; }       // Score calculé
    }
}
