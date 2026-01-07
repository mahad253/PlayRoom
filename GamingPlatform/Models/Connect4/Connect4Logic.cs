namespace GamingPlatform.Models.Connect4;

public static class Connect4Logic
{
    public static int FindDropRow(Cell[,] board, int col)
    {
        for (int r = 5; r >= 0; r--)
            if (board[r, col] == Cell.Empty) return r;
        return -1;
    }

    public static bool HasWon(Cell[,] b, int row, int col, Cell who)
    {
        return CountLine(b, row, col, 0, 1, who) >= 4   // horizontal
            || CountLine(b, row, col, 1, 0, who) >= 4   // vertical
            || CountLine(b, row, col, 1, 1, who) >= 4   // diag \
            || CountLine(b, row, col, 1, -1, who) >= 4; // diag /
    }

    private static int CountLine(Cell[,] b, int r, int c, int dr, int dc, Cell who)
    {
        int count = 1;

        count += CountDir(b, r, c, dr, dc, who);
        count += CountDir(b, r, c, -dr, -dc, who);

        return count;
    }

    private static int CountDir(Cell[,] b, int r, int c, int dr, int dc, Cell who)
    {
        int cnt = 0;
        int rr = r + dr, cc = c + dc;

        while (rr >= 0 && rr < 6 && cc >= 0 && cc < 7 && b[rr, cc] == who)
        {
            cnt++;
            rr += dr; cc += dc;
        }
        return cnt;
    }
}
