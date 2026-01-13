using System.Collections.Generic;
using System.Threading.Tasks;
using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;

public static class SentenceSeeder
{
    public static async Task SeedAsync(GamingPlatformContext context)
    {
        // S'il y a déjà des phrases, on ne fait rien
        if (await context.Sentences.AnyAsync())
            return;

        var sentences = new List<Sentence>
        {
            new Sentence
            {
                Text = "Bonjour tous le monde.",
                Language = "FR",
                Difficulty = 1
            },
            new Sentence
            {
                Text = "La vitesse de frappe ne se mesure pas seulement en mots par minute, mais aussi en précision.\nUn bon joueur de speedtyping prend le temps de regarder ses erreurs, de les comprendre, puis de s'entraîner à les éviter.\nAvec quelques minutes de pratique chaque jour, les progrès deviennent rapidement visibles.",
                Language = "FR",
                Difficulty = 2
            },
            new Sentence
            {
                Text = "Dans un jeu de speedtyping, l'objectif n'est pas uniquement de frapper les touches le plus rapidement possible.\nIl s'agit surtout de maintenir un équilibre délicat entre vitesse et exactitude, tout en restant concentré sur chaque détail du texte.\nPlus le paragraphe contient de mots rares, d'accents et de ponctuation, plus le défi devient intéressant pour les joueurs expérimentés.",
                Language = "FR",
                Difficulty = 3
            }
        };

        context.Sentences.AddRange(sentences);
        await context.SaveChangesAsync();
    }
}