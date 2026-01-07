namespace GamingPlatform.Models
{
    public class LeaderboardViewModel
{
    public string GameType { get; set; }
    public List<ScoreViewModel> Scores { get; set; }
}

public class ScoreViewModel
{
    public int Rank { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public DateTime Date { get; set; }
}

}
