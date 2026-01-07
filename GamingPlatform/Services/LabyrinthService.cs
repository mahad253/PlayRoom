using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Services
{
	public class LabyrinthService
	{
		private readonly GamingPlatformContext _dbContext;

		public LabyrinthService(GamingPlatformContext dbContext)
		{
			_dbContext = dbContext;
		}

        public async Task<Labyrinth> CreateLabyrinthAsync(Guid lobbyId, string player1, string player2, string data)
        {
            // Vérifier si le lobby existe
            var lobby = await _dbContext.Lobby
                .Include(l => l.Game) // Inclure les jeux si nécessaire
                .FirstOrDefaultAsync(l => l.Id == lobbyId);

            if (lobby == null)
            {
                throw new InvalidOperationException("Le lobby n'existe pas.");
            }

            // Créer le labyrinthe (ici, vous pouvez ajouter la logique de génération du labyrinthe)
            var labyrinth = new Labyrinth
            {
                LobbyId = lobbyId,
                player1 = player1,
                player2 = player2,
                statep1 = "initial",  // Initialiser l'état des joueurs (par exemple, au début du jeu)
                statep2 = "initial",
                Data = data, // Générer les données du labyrinthe (en JSON par exemple)
                CreatedAt = DateTime.Now
            };

            // Ajouter le labyrinthe à la base de données
            _dbContext.Labyrinth.Add(labyrinth);
            await _dbContext.SaveChangesAsync();

            return labyrinth;
        }

        public async Task<Labyrinth> GetLabyrinthByIdAsync(int id)
		{
			return await _dbContext.Labyrinth.FindAsync(id);
		}
        public async Task<Labyrinth> GetLabyrinthByLobbyIdAsync(Guid lobbyId)
        {
            return await _dbContext.Labyrinth
                .FirstOrDefaultAsync(l => l.LobbyId == lobbyId);
        }
        // Vérifier si un labyrinthe existe pour un lobby donné
        public async Task<bool> IsLabyrinthGeneratedForLobbyAsync(Guid lobbyId)
        {
            return await _dbContext.Labyrinth.AnyAsync(l => l.LobbyId == lobbyId);
        }
    }
}
