using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingPlatform.Models
{
    /// <summary>
    /// Représente une partie de SpeedTyping (sans lobby).
    /// </summary>
    [Table("SpeedTypingGames")]
    public class SpeedTypingGame
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Token unique pour identifier la partie (utile pour l'URL ou SignalR).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string GameToken { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Phrase utilisée pour cette partie.
        /// </summary>
        [Required]
        public int SentenceId { get; set; }

        [ForeignKey(nameof(SentenceId))]
        public Sentence? Sentence { get; set; }

        /// <summary>
        /// Copie du texte au moment de la partie (pour l'historique).
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string TextToType { get; set; } = default!;

        /// <summary>
        /// Difficulté : Easy, Medium, Hard.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Difficulty { get; set; } = "Easy";

        /// <summary>
        /// Durée prévue de la partie en secondes.
        /// </summary>
        [Range(10, 600)]
        public int DurationSeconds { get; set; } = 60;

        /// <summary>
        /// Nombre de joueurs ayant participé à cette partie (ou 1 si solo).
        /// </summary>
        [Range(1, 100)]
        public int ParticipantCount { get; set; } = 1;

        /// <summary>
        /// Statut de la partie : Waiting, InProgress, Finished.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Waiting";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Pseudo du gagnant (pour le multi, ou le joueur lui-même en solo).
        /// </summary>
        [MaxLength(100)]
        public string? WinnerPseudo { get; set; }

        /// <summary>
        /// Score du gagnant.
        /// </summary>
        public int? WinnerScore { get; set; }

        /// <summary>
        /// Scores de tous les joueurs pour cette partie.
        /// </summary>
        public ICollection<SpeedTypingScore> Scores { get; set; } = new List<SpeedTypingScore>();
    }
}
