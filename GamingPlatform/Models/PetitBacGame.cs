using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GamingPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace GamingPlatform.Models
{
    public class PetitBacGame
    {
        public int Id { get; set; } // Identifiant unique de la partie
        public List<char> Letters { get; set; } = new List<char>();
        public bool IsGameStarted { get; set; } = false; // Indique si la partie a commencé
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Date de création
        
        public string CreatorPseudo { get; set; } // Pseudo du créateur de la partie
        public int PlayerCount { get; set; } // Nombre total de joueurs pour la partie

        // Liste des catégories choisies
        public List<PetitBacCategory> Categories { get; set; } = new List<PetitBacCategory>();

        [NotMapped]
        public List<string> AvailableCategories { get; set; } = new List<string>
        {
            "Pays", "Animaux", "Fruits", "Prénoms", "Villes", "Métiers",
            "Couleurs", "Sports", "Objets", "Plantes", "Marques",
            "Fleurs", "Langues", "Instruments de musique", "Films",
            "Séries", "Livres", "Paysages", "Nourriture", "Boissons",
            "Vêtements", "Personnages célèbres", "Desserts", "Appareils électroniques", 
            "Monuments célèbres", "Jeux vidéo", "Personnages historiques",
            "Danses", "Fêtes ou célébrations"
        };

        // Liste des joueurs associés à la partie
        public List<PetitBacPlayer> Players { get; set; } = new List<PetitBacPlayer>();

        // Relation avec le lobby
        public Guid LobbyId { get; set; }
        public Lobby Lobby { get; set; } // Le lobby associé à cette partie
    }
}
