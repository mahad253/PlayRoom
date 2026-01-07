namespace GamingPlatform.Models
{
    public interface IGameBoard
    {
        void InitializeBoard(); // Initialiser le plateau de jeu
        string RenderBoard();   // Rendu du plateau sous forme de chaîne de caractères, ou HTML pour l'interface
        bool IsGameOver();      // Vérifie si la partie est terminée
    }

}
