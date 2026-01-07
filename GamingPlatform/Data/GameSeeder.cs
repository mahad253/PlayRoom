

using GamingPlatform.Models;

namespace GamingPlatform.Data
{
    public class GameSeeder
    {
        private readonly GamingPlatformContext _context;

        public GameSeeder(GamingPlatformContext context)
        {
            _context = context;
        }

		public void SeedGames()
		{
			// Liste des jeux à insérer ou mettre à jour
			var games = new List<Game>
	{
		new Game { 
			Code = "SPT", Name = "SpeedTyping", 
			Description = 
			"Le jeu Speed Typing est un jeu de dactylographie. C’est une activité interactive qui consiste à taper les mots qui apparaissent à l’écran, le plus correctement et le plus rapidement possible", ImageUrl = "/images/speedtyping.png" },
		new Game { Code = "MOR", Name = "Morpion", 
		Description = "Le morpion est un jeu de réflexion se pratiquant à deux joueurs au tour par tour et dont le but est de créer le premier un alignement sur une grille. Le jeu se joue généralement avec papier et crayon.", ImageUrl = "/images/morpion.png" },
		//new Game { Code = "BTN", Name = "BatailleNavale", Description = "Un jeu de stratégie navale.", ImageUrl = "/images/bataillenavale.png" },
		new Game { Code = "PTB", Name = "PetitBac", 
		Description = "Le Petit Bac repose sur la rapidité et la connaissance générale des joueurs. L’objectif est de remplir des catégories prédéterminées avec des mots commençant par une lettre choisie. Les joueurs doivent trouver le maximum de réponses correctes.", ImageUrl = "/images/petitbac.png" },
		new Game
		{
			Code = "LAB",
			Name = "Course de Labyrinthe",
			Description = "Dans Labyrinthe Course, deux joueurs s'affrontent dans une course à travers un labyrinthe fixe. L’objectif est simple : être le premier à atteindre le point " +
			"de sortie en naviguant à travers les couloirs sinueux du labyrinthe. Les joueurs doivent non seulement avancer rapidement, mais aussi anticiper les mouvements de leur adversaire et " +
			"choisir les meilleures trajectoires. La partie met en avant à la fois la stratégie de déplacement et la prise de décision rapide.",
			ImageUrl = "/images/labyrinthe.png"
		}
	};

			// Parcourir les jeux pour vérifier leur existence ou les mettre à jour
			foreach (var game in games)
			{
				var existingGame = _context.Game.FirstOrDefault(g => g.Code == game.Code);
				if (existingGame == null)
				{
					// Ajouter un nouveau jeu si non trouvé
					_context.Game.Add(new Game
					{
						Id = Guid.NewGuid(),
						Code = game.Code,
						Name = game.Name,
						Description = game.Description,
						ImageUrl = game.ImageUrl
					});
				}
				else
				{
					// Mettre à jour les propriétés du jeu existant si nécessaire
					existingGame.Name = game.Name;
					existingGame.Description = game.Description;
					existingGame.ImageUrl = game.ImageUrl;
				}
			}

			// Sauvegarder les modifications dans la base de données
			_context.SaveChanges();
		}

	}
}
