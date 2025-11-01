using Azure;
using Azure.Data.Tables;

namespace PoTicTacServer.Models;

public class PlayerStats
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

public class PlayerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "players";
    public string RowKey { get; set; } = string.Empty; // Will be the lowercase player name
    public string Name { get; set; } = string.Empty;
    public string Stats { get; set; } = string.Empty; // JSON serialized PlayerStats
    public string LastUpdated { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
