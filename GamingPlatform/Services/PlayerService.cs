using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamingPlatform.Models;
using GamingPlatform.Data;

namespace GamingPlatform.Services
{
    public class PlayerService
    {
        private readonly GamingPlatformContext _context;

        // Constructeur du service pour injecter le DbContext
        public PlayerService(GamingPlatformContext context)
        {
            _context = context;
        }

        // Méthode pour ajouter un nouveau joueur
        public async Task<Player> AddPlayerAsync(string pseudo, string name = null, string email = null)
        {
            // Vérifier si le pseudo existe déjà
            var existingPlayer = await _context.Player
                .FirstOrDefaultAsync(p => p.Pseudo == pseudo);
            if (existingPlayer != null)
            {
                return null; // Le pseudo est déjà pris
            }

            // Créer un nouveau joueur
            var player = new Player
            {
                Pseudo = pseudo,
                Name = name,
                email = email
            };

            _context.Player.Add(player);
            await _context.SaveChangesAsync();

            return player;
        }

        // Méthode pour récupérer un joueur par son ID (Guid)
        public async Task<Player> GetPlayerByIdAsync(int playerId)
        {
            // Recherche du joueur par son ID dans la base de données
            return await _context.Player
                .FirstOrDefaultAsync(p => p.Id == playerId);
        }

        // Méthode pour récupérer un joueur par son pseudo
        public async Task<Player> GetPlayerByPseudoAsync(string pseudo)
        {
            return await _context.Player
                .FirstOrDefaultAsync(p => p.Pseudo == pseudo);
        }

        // Méthode pour récupérer tous les joueurs
        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _context.Player.ToListAsync();
        }

        // Méthode pour récupérer les lobbies d'un joueur spécifique
        public async Task<List<Lobby>> GetLobbiesByPlayerAsync(int playerId)
        {
            var player = await _context.Player
                .Include(p => p.LobbyPlayers)
                .ThenInclude(lp => lp.Lobby)
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
            {
                return new List<Lobby>();
            }

            return player.LobbyPlayers.Select(lp => lp.Lobby).ToList();
        }

        // Méthode pour supprimer un joueur
        public async Task<bool> DeletePlayerAsync(Guid playerId)
        {
            var player = await _context.Player.FindAsync(playerId);
            if (player == null)
            {
                return false; // Le joueur n'existe pas
            }

            _context.Player.Remove(player);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
