using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Data
{
    public class GamingPlatformContext : DbContext
    {
        public DbSet<Sentence> Sentences { get; set; } = default!;
        public DbSet<Lobby> Lobbies { get; set; }
        public DbSet<LobbyPlayer> LobbyPlayers { get; set; }


        public GamingPlatformContext(DbContextOptions<GamingPlatformContext> options)
            : base(options)
        {
        }
        
    }
}