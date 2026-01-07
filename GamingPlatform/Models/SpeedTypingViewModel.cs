namespace GamingPlatform.Models
{
    public class SpeedTypingViewModel
    {
       public Guid LobbyId { get; set; }
        public string Name { get; set; }
        public List<string> PlayerPseudos { get; set; }
        public string CurrentPlayerPseudo { get; set; }
    }
}
