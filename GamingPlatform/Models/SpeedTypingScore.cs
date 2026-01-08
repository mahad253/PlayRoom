using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingPlatform.Models
{
    /// <summary>
    /// Représente le score d'un joueur pour une partie de SpeedTyping.
    /// </summary>
    [Table("SpeedTypingScores")]
    public class SpeedTypingScore
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Partie à laquelle ce score est associé.
        /// </summary>
        [Required]
        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public SpeedTypingGame? Game { get; set; }

        /// <summary>
        /// Identifiant du joueur (ou pseudo s'il n'y a pas d'authentification).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PlayerId { get; set; } = default!;

        /// <summary>
        /// Pseudo affiché pour ce score.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Pseudo { get; set; } = default!;

        /// <summary>
        /// Difficulté de la partie (copie pour simplifier les requêtes).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Difficulty { get; set; } = "Easy";

        /// <summary>
        /// Vitesse de frappe (Words Per Minute).
        /// </summary>
        [Range(0, 1000)]
        public int Wpm { get; set; }

        /// <summary>
        /// Précision en pourcentage (0–100).
        /// </summary>
        [Range(0, 100)]
        public double Accuracy { get; set; }

        /// <summary>
        /// Score final, par exemple WPM * (Accuracy / 100).
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Score { get; set; }

        /// <summary>
        /// Nombre total de caractères tapés.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CharactersTyped { get; set; }

        /// <summary>
        /// Nombre de caractères corrects.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CharactersCorrect { get; set; }

        /// <summary>
        /// Nombre de caractères incorrects.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CharactersWrong { get; set; }

        /// <summary>
        /// Durée réelle du test en secondes.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Indique si ce joueur est le gagnant de la partie.
        /// </summary>
        public bool IsWinner { get; set; } = false;

        /// <summary>
        /// Rang du joueur dans cette partie (1 = premier, 2 = deuxième, etc.).
        /// </summary>
        [Range(1, 100)]
        public int RankInGame { get; set; } = 1;

        /// <summary>
        /// Date et heure à laquelle la partie a été jouée.
        /// </summary>
        public DateTime DatePlayed { get; set; } = DateTime.UtcNow;
    }
}
