using FluentAssertions;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;

namespace Po.TicTac.UnitTests;

public class StatsSummaryModelTests
{
    [Fact]
    public void FromPlayers_WithNullCollection_ReturnsDefaultSummary()
    {
        var summary = StatsSummaryModel.FromPlayers(null);

        summary.PlayerCount.Should().Be(0);
        summary.TotalGames.Should().Be(0);
        summary.TotalWins.Should().Be(0);
        summary.TotalDraws.Should().Be(0);
        summary.AverageWinRate.Should().Be(0);
        summary.TopPlayerName.Should().Be("N/A");
        summary.TopPlayerWinRate.Should().Be(0);
        summary.LongestWinStreak.Should().Be(0);
    }

    [Fact]
    public void FromPlayers_AggregatesTotalsAndSelectsTopPlayer()
    {
        var players = new List<PlayerStatsDto>
        {
            new()
            {
                Name = "Alice",
                Stats = new PlayerStats
                {
                    TotalWins = 10,
                    TotalDraws = 2,
                    TotalGames = 18,
                    OverallWinRate = 0.65,
                    Easy = new DifficultyStats { WinStreak = 3 },
                    Medium = new DifficultyStats { WinStreak = 4 },
                    Hard = new DifficultyStats { WinStreak = 2 }
                }
            },
            new()
            {
                Name = "Bob",
                Stats = new PlayerStats
                {
                    TotalWins = 8,
                    TotalDraws = 1,
                    TotalGames = 15,
                    OverallWinRate = 0.7,
                    Easy = new DifficultyStats { WinStreak = 5 },
                    Medium = new DifficultyStats { WinStreak = 1 },
                    Hard = new DifficultyStats { WinStreak = 6 }
                }
            },
            new()
            {
                Name = "Charlie",
                Stats = new PlayerStats
                {
                    TotalWins = 12,
                    TotalDraws = 3,
                    TotalGames = 25,
                    OverallWinRate = 0.7,
                    Easy = new DifficultyStats { WinStreak = 1 },
                    Medium = new DifficultyStats { WinStreak = 2 },
                    Hard = new DifficultyStats { WinStreak = 9 }
                }
            }
        };

        var summary = StatsSummaryModel.FromPlayers(players);

        summary.PlayerCount.Should().Be(3);
        summary.TotalGames.Should().Be(58);
        summary.TotalWins.Should().Be(30);
        summary.TotalDraws.Should().Be(6);
        summary.AverageWinRate.Should().BeApproximately(0.6833, 0.0001);
        summary.TopPlayerName.Should().Be("Charlie");
        summary.TopPlayerWinRate.Should().BeApproximately(0.7, 0.0001);
        summary.LongestWinStreak.Should().Be(9);
    }

    [Fact]
    public void FromPlayers_ClampsInvalidWinRates()
    {
        var players = new List<PlayerStatsDto>
        {
            new()
            {
                Name = "Delta",
                Stats = new PlayerStats
                {
                    TotalWins = 5,
                    TotalDraws = 0,
                    TotalGames = 5,
                    OverallWinRate = 1.2,
                    Easy = new DifficultyStats { WinStreak = 1 },
                    Medium = new DifficultyStats { WinStreak = 0 },
                    Hard = new DifficultyStats { WinStreak = 0 }
                }
            },
            new()
            {
                Name = "Echo",
                Stats = new PlayerStats
                {
                    TotalWins = 0,
                    TotalDraws = 0,
                    TotalGames = 4,
                    OverallWinRate = -0.4,
                    Easy = new DifficultyStats { WinStreak = 2 },
                    Medium = new DifficultyStats { WinStreak = 3 },
                    Hard = new DifficultyStats { WinStreak = 4 }
                }
            }
        };

        var summary = StatsSummaryModel.FromPlayers(players);

        summary.AverageWinRate.Should().BeApproximately(0.5, 0.0001);
        summary.TopPlayerName.Should().Be("Delta");
        summary.TopPlayerWinRate.Should().Be(1);
    }
}
