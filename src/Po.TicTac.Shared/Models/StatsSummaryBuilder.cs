using Po.TicTac.Shared.DTOs;

namespace Po.TicTac.Shared.Models;

/// <summary>
/// Builds aggregate statistics for the statistics dashboard.
/// </summary>
public static class StatsSummaryBuilder
{
    public static StatsSummaryModel FromPlayers(IEnumerable<PlayerStatsDto>? players)
    {
        if (players is null)
        {
            return new StatsSummaryModel();
        }

        var playerList = players.ToList();
        if (playerList.Count == 0)
        {
            return new StatsSummaryModel();
        }

        var totalWins = playerList.Sum(p => p.Stats.TotalWins);
        var totalDraws = playerList.Sum(p => p.Stats.TotalDraws);
        var totalGames = playerList.Sum(p => p.Stats.TotalGames);
        var averageWinRate = playerList.Average(p => ClampRate(p.Stats.OverallWinRate));

        var topPlayer = playerList
            .Where(p => p.Stats.TotalGames > 0)
            .OrderByDescending(p => ClampRate(p.Stats.OverallWinRate))
            .ThenByDescending(p => p.Stats.TotalGames)
            .FirstOrDefault();

        var longestWinStreak = playerList
            .SelectMany(p => new[]
            {
                p.Stats.Easy.WinStreak,
                p.Stats.Medium.WinStreak,
                p.Stats.Hard.WinStreak
            })
            .DefaultIfEmpty(0)
            .Max();

        return new StatsSummaryModel
        {
            PlayerCount = playerList.Count,
            TotalGames = totalGames,
            TotalWins = totalWins,
            TotalDraws = totalDraws,
            AverageWinRate = averageWinRate,
            TopPlayerName = topPlayer?.Name ?? "N/A",
            TopPlayerWinRate = topPlayer is null ? 0 : ClampRate(topPlayer.Stats.OverallWinRate),
            LongestWinStreak = longestWinStreak
        };
    }

    private static double ClampRate(double value)
    {
        if (!double.IsFinite(value))
        {
            return 0;
        }

        return Math.Clamp(value, 0, 1);
    }
}
