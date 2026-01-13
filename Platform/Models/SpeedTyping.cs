namespace PlayRoom.Models;


public class SpeedTypingPlayerState
{
    public string ConnectionId { get; set; } = default!;
    public string Pseudo { get; set; } = default!;
    public int CorrectChars { get; set; }
    public int TotalTyped { get; set; }
    public bool Finished { get; set; }
    public double Wpm { get; set; }
    public double Accuracy => TotalTyped == 0 ? 0 : (double)CorrectChars / TotalTyped * 100.0;
}

public class SpeedTypingGame
{
    public string TextToType { get; private set; } = "";
    public int DurationSeconds { get; private set; } = 60;
    public DateTime? StartTime { get; private set; }
    public bool Started => StartTime.HasValue;
    public bool Finished { get; private set; }

    public Dictionary<string, SpeedTypingPlayerState> Players { get; } = new();

    public void Init(string text, int durationSeconds, IEnumerable<(string connectionId, string pseudo)> players)
    {
        TextToType = text;
        DurationSeconds = durationSeconds;
        StartTime = null;
        Finished = false;
        Players.Clear();

        foreach (var p in players)
        {
            Players[p.connectionId] = new SpeedTypingPlayerState
            {
                ConnectionId = p.connectionId,
                Pseudo = p.pseudo
            };
        }
    }

    public void Start()
    {
        if (Started) return;
        StartTime = DateTime.UtcNow;
    }

    public void UpdateProgress(string connectionId, int totalTyped, int correctChars)
    {
        if (!Started || Finished) return;
        if (!Players.TryGetValue(connectionId, out var state)) return;

        state.TotalTyped = totalTyped;
        state.CorrectChars = correctChars;

        var elapsedSeconds = (DateTime.UtcNow - StartTime!.Value).TotalSeconds;
        if (elapsedSeconds <= 0) elapsedSeconds = 1;

        state.Wpm = (correctChars / 5.0) * (60.0 / elapsedSeconds);

        if (elapsedSeconds >= DurationSeconds)
        {
            Finished = true;
        }
    }
}
