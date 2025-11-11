namespace Po.TicTac.Shared.Models;

public class DifficultyStats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalGames { get; set; }
    public int WinStreak { get; set; }
    public int CurrentStreak { get; set; }
    public double AverageMovesPerGame { get; set; }
    public int TotalMoves { get; set; }
    public double WinRate { get; set; }
}

public class PlayerStats
{
    public DifficultyStats Easy { get; set; } = new DifficultyStats();
    public DifficultyStats Medium { get; set; } = new DifficultyStats();
    public DifficultyStats Hard { get; set; } = new DifficultyStats();

    // Overall stats across all difficulties
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int TotalDraws { get; set; }
    public int TotalGames { get; set; }
    public double OverallWinRate { get; set; }
}
