using System.Collections.Generic;

namespace GamingPlatform.Models
{
    public class GameDetailsViewModel
    {
        public Game Game { get; set; } // Informations sur le jeu
        public List<Lobby> Lobbies { get; set; } // Liste des lobbies associés
    }
}
