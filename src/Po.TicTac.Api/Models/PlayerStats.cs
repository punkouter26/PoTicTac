namespace Po.TicTac.Api.Models;

/// <summary>
/// Statistics for a specific difficulty level.
/// </summary>
public sealed class DifficultyStats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalGames { get; set; }
    public int WinStreak { get; set; }
    public double WinRate { get; set; }
}

/// <summary>
/// Player statistics stored in Azure Table Storage.
/// </summary>
public sealed class PlayerStats
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public DifficultyStats Easy { get; set; } = new();
    public DifficultyStats Medium { get; set; } = new();
    public DifficultyStats Hard { get; set; } = new();
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int TotalDraws { get; set; }
    public int TotalGames { get; set; }
    public double WinRate { get; set; }
    public double OverallWinRate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
