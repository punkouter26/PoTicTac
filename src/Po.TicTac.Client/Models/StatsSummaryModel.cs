namespace PoTicTac.Client.Models;

/// <summary>
/// Aggregated statistics derived from the player statistics collection.
/// </summary>
public sealed class StatsSummaryModel
{
    public int PlayerCount { get; init; }

    public int TotalGames { get; init; }

    public int TotalWins { get; init; }

    public int TotalDraws { get; init; }

    public double AverageWinRate { get; init; }

    public string TopPlayerName { get; init; } = "N/A";

    public double TopPlayerWinRate { get; init; }

    public int LongestWinStreak { get; init; }
}
