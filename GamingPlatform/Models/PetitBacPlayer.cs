using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GamingPlatform.Models
{
    public class PetitBacPlayer
    {
        public int Id { get; set; } // Identifiant unique du joueur
        public string Pseudo { get; set; } // Pseudo du joueur
        public string Status { get; set; } = "Inactive"; 
         // Jeton unique pour sécuriser l'accès à la session du joueur
        public string SessionToken { get; set; } = Guid.NewGuid().ToString();

        // Stockage des réponses sous forme de JSON dans la base de données
        [NotMapped]
        public Dictionary<char, Dictionary<string, string>> Responses { get; set; }

public string ResponsesJson
{
    get => JsonSerializer.Serialize(Responses);
    set => Responses = string.IsNullOrEmpty(value)
        ? new Dictionary<char, Dictionary<string, string>>()
        : JsonSerializer.Deserialize<Dictionary<char, Dictionary<string, string>>>(value);
}

        public double Score { get; set; }  // Score du joueur pour la partie
        public bool IsReady { get; set; } = false; // Indique si le joueur est prêt pour commencer

        // Date d'inscription à la partie
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Relation avec PetitBacGame
        public int PetitBacGameId { get; set; } // Clé étrangère vers la partie
        public PetitBacGame PetitBacGame { get; set; } // Référence à la partie associée
    }
}
