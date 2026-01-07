namespace GamingPlatform.Models
{
    public class TypingGameViewModel
    {
        /// <summary>
        /// Niveau de difficulté choisi (Facile, Moyen, Difficile).
        /// </summary>
        public string Difficulty { get; set; } = "Facile";

        /// <summary>
        /// Texte que le joueur doit saisir.
        /// </summary>
        public string TextToType { get; set; } =
            "Practice makes perfect. The quick brown fox jumps over the lazy dog. All that glitters is not gold.";

        /// <summary>
        /// Durée du test en secondes.
        /// </summary>
        public int TimeLimitSeconds { get; set; } = 60;

        /// <summary>
        /// Progression en pourcentage (0 à 100).
        /// </summary>
        public int Progress { get; set; } = 0;

        /// <summary>
        /// Vitesse en mots par minute calculée à la fin de la partie.
        /// </summary>
        public int Wpm { get; set; }

        /// <summary>
        /// Précision en pourcentage.
        /// </summary>
        public int Accuracy { get; set; }
    }
}