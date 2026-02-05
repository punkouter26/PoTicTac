using Po.TicTac.Api.DTOs;

namespace Po.TicTac.Api.Models;

/// <summary>
/// Summary statistics across all players.
/// </summary>
public sealed class StatsSummaryModel
{
    public int PlayerCount { get; set; }
    public int TotalGames { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int TotalDraws { get; set; }
    public double OverallWinRate { get; set; }
    public double AverageWinRate { get; set; }
    public string TopPlayerName { get; set; } = "N/A";
    public int TopPlayerWins { get; set; }
    public double TopPlayerWinRate { get; set; }
    public int LongestWinStreak { get; set; }

    /// <summary>
    /// Creates a summary from player statistics DTOs.
    /// </summary>
    public static StatsSummaryModel FromPlayers(IEnumerable<PlayerStatsDto>? players)
    {
        if (players is null)
            return new StatsSummaryModel();

        var playersList = players.ToList();
        if (playersList.Count == 0)
            return new StatsSummaryModel();

        var totalGames = playersList.Sum(p => p.Stats.TotalGames);
        var totalWins = playersList.Sum(p => p.Stats.TotalWins);
        var totalDraws = playersList.Sum(p => p.Stats.TotalDraws);

        var clampedWinRates = playersList
            .Select(p => Math.Clamp(p.Stats.OverallWinRate, 0, 1))
            .ToList();
        var averageWinRate = clampedWinRates.Count > 0 ? clampedWinRates.Average() : 0;

        var topPlayer = playersList
            .OrderByDescending(p => Math.Clamp(p.Stats.OverallWinRate, 0, 1))
            .ThenByDescending(p => p.Stats.TotalWins)
            .FirstOrDefault();

        var topPlayerWinRate = topPlayer is not null 
            ? Math.Clamp(topPlayer.Stats.OverallWinRate, 0, 1) 
            : 0;

        var longestWinStreak = playersList
            .SelectMany(p => new[] { p.Stats.Easy.WinStreak, p.Stats.Medium.WinStreak, p.Stats.Hard.WinStreak })
            .DefaultIfEmpty(0)
            .Max();

        return new StatsSummaryModel
        {
            PlayerCount = playersList.Count,
            TotalGames = totalGames,
            TotalWins = totalWins,
            TotalDraws = totalDraws,
            AverageWinRate = averageWinRate,
            TopPlayerName = topPlayer?.Name ?? "N/A",
            TopPlayerWinRate = topPlayerWinRate,
            LongestWinStreak = longestWinStreak
        };
    }
}
