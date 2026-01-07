namespace GamingPlatform.Models
{
    public class Labyrinth
    {
        public int Id { get; set; }
        public Guid LobbyId { get; set; }
        public Lobby Lobby { get; set; }
        public string player1 { get; set; }
        public string player2 { get; set; }
        public string statep1 { get; set; }
        public string statep2 { get; set; }
        public string Data { get; set; } // Stocke les données du labyrinthe en JSON
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}

}
