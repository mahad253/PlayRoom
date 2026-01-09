using System.Linq;

namespace GamingPlatform.Models
{
    public class Morpion
    {
        public string[] Board { get; private set; } = new string[9];
        public string CurrentPlayer { get; private set; } = "X";
        public bool Finished { get; private set; }
        public string? Winner { get; private set; }

        public Morpion()
        {
            Reset();
        }

        public void Play(int index)
        {
            // Sécurités STRICTES
            if (Finished)
                return;

            if (index < 0 || index >= 9)
                return;

            if (!string.IsNullOrEmpty(Board[index]))
                return;

            Board[index] = CurrentPlayer;

            CheckWinner();

            if (!Finished)
                CurrentPlayer = CurrentPlayer == "X" ? "O" : "X";
        }

        public void Reset()
        {
            for (int i = 0; i < 9; i++)
                Board[i] = "";

            CurrentPlayer = "X";
            Finished = false;
            Winner = null;
        }

        private void CheckWinner()
        {
            int[,] wins =
            {
                {0,1,2},{3,4,5},{6,7,8},
                {0,3,6},{1,4,7},{2,5,8},
                {0,4,8},{2,4,6}
            };

            for (int i = 0; i < wins.GetLength(0); i++)
            {
                string a = Board[wins[i, 0]];
                string b = Board[wins[i, 1]];
                string c = Board[wins[i, 2]];

                if (!string.IsNullOrEmpty(a) && a == b && b == c)
                {
                    Winner = a;
                    Finished = true;
                    return;
                }
            }

            // Match nul
            if (Board.All(x => !string.IsNullOrEmpty(x)))
            {
                Winner = "Draw";
                Finished = true;
            }
        }
    }
}
