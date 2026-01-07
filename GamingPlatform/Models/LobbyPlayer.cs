using System;

namespace GamingPlatform.Models
{
    public class LobbyPlayer
    {
        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public Guid LobbyId { get; set; }
        public Lobby Lobby { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}

