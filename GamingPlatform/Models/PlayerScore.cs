namespace GamingPlatform.Models
{
public class PlayerScore
{
    public string PlayerId { get; set; }
    public string Pseudo { get; set; }
    public int WPM { get; set; }
    public double Accuracy { get; set; }
    public int Score { get; set; }
    public Difficulty Difficulty { get; set; }
}


}