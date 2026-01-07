using System.Linq;

namespace GamingPlatform.Models
{
    public class Morpion
    {
        public string[] Board { get; set; } = new string[9];
        public string CurrentPlayer { get; set; } = "X";
        public bool Finished { get; set; }
        public string? Winner { get; set; }

        public Morpion()
        {
            for (int i = 0; i < 9; i++)
                Board[i] = "";
        }

        public void Play(int index)
        {
            // Sécurité indispensable
            if (Finished)
                return;

            if (index < 0 || index >= 9)
                return;

            if (Board[index] != "")
                return;

            Board[index] = CurrentPlayer;
            CheckWinner();

            if (!Finished)
                CurrentPlayer = CurrentPlayer == "X" ? "O" : "X";
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

                if (a != "" && a == b && b == c)
                {
                    Winner = a;
                    Finished = true;
                    return;
                }
            }

            if (Board.All(x => x != ""))
            {
                Winner = "Draw";
                Finished = true;
            }
        }
    }
}
