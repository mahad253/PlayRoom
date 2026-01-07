namespace GamingPlatform.Models
{
    public class ConfirmationViewModel
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public string PlayerPseudo { get; set; }
        public List<string> Categories { get; set; }
        public double Score { get; set; } 
    }
}
