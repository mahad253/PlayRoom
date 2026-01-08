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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sentence>().HasData(
                new Sentence
                {
                    Id = 1,
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Language = "EN",
                    Difficulty = 1
                },
                new Sentence
                {
                    Id = 2,
                    Text = "Bonjour, comment allez-vous aujourd'hui ?",
                    Language = "FR",
                    Difficulty = 1
                },
                new Sentence
                {
                    Id = 3,
                    Text = "La vie est une série de choix qui façonnent notre avenir.",
                    Language = "FR",
                    Difficulty = 2
                }
            );
        }
    }
}