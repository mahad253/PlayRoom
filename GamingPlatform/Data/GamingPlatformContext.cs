using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Data
{
    public class GamingPlatformContext : DbContext
    {
        public DbSet<Sentence> Sentences { get; set; } = default!;

        public GamingPlatformContext(DbContextOptions<GamingPlatformContext> options)
            : base(options)
        {
        }
        
    }
}