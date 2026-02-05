using FluentAssertions;
using Po.TicTac.Api.Models;

namespace Po.TicTac.UnitTests;

/// <summary>
/// Unit tests for PlayerStats and DifficultyStats domain models.
/// Tests pure logic and domain rules without external dependencies.
/// </summary>
public class PlayerStatsTests
{
    #region DifficultyStats Tests

    [Fact]
    public void DifficultyStats_DefaultInitialization_HasZeroValues()
    {
        // Arrange & Act
        var stats = new DifficultyStats();

        // Assert
        stats.Wins.Should().Be(0);
        stats.Losses.Should().Be(0);
        stats.Draws.Should().Be(0);
        stats.TotalGames.Should().Be(0);
        stats.WinStreak.Should().Be(0);
        stats.WinRate.Should().Be(0);
    }

    [Fact]
    public void DifficultyStats_CanSetAllProperties()
    {
        // Arrange & Act
        var stats = new DifficultyStats
        {
            Wins = 10,
            Losses = 5,
            Draws = 3,
            TotalGames = 18,
            WinStreak = 4,
            WinRate = 0.556
        };

        // Assert
        stats.Wins.Should().Be(10);
        stats.Losses.Should().Be(5);
        stats.Draws.Should().Be(3);
        stats.TotalGames.Should().Be(18);
        stats.WinStreak.Should().Be(4);
        stats.WinRate.Should().BeApproximately(0.556, 0.001);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 0, 0, 1)]
    [InlineData(5, 5, 0, 10)]
    [InlineData(10, 5, 5, 20)]
    public void DifficultyStats_TotalGames_ShouldMatchSumOfOutcomes(int wins, int losses, int draws, int expectedTotal)
    {
        // Arrange
        var stats = new DifficultyStats
        {
            Wins = wins,
            Losses = losses,
            Draws = draws,
            TotalGames = expectedTotal
        };

        // Assert
        stats.TotalGames.Should().Be(wins + losses + draws);
    }

    #endregion

    #region PlayerStats Tests

    [Fact]
    public void PlayerStats_DefaultInitialization_HasEmptyValues()
    {
        // Arrange & Act
        var stats = new PlayerStats();

        // Assert
        stats.PlayerId.Should().BeEmpty();
        stats.PlayerName.Should().BeEmpty();
        stats.TotalWins.Should().Be(0);
        stats.TotalLosses.Should().Be(0);
        stats.TotalDraws.Should().Be(0);
        stats.TotalGames.Should().Be(0);
        stats.WinRate.Should().Be(0);
        stats.OverallWinRate.Should().Be(0);
        stats.Easy.Should().NotBeNull();
        stats.Medium.Should().NotBeNull();
        stats.Hard.Should().NotBeNull();
    }

    [Fact]
    public void PlayerStats_DifficultyStats_AreIndependent()
    {
        // Arrange
        var stats = new PlayerStats();

        // Act
        stats.Easy.Wins = 5;
        stats.Medium.Wins = 10;
        stats.Hard.Wins = 15;

        // Assert
        stats.Easy.Wins.Should().Be(5);
        stats.Medium.Wins.Should().Be(10);
        stats.Hard.Wins.Should().Be(15);
        stats.Easy.Should().NotBeSameAs(stats.Medium);
        stats.Medium.Should().NotBeSameAs(stats.Hard);
    }

    [Fact]
    public void PlayerStats_CreatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var stats = new PlayerStats();

        // Assert
        var after = DateTime.UtcNow;
        stats.CreatedAt.Should().BeOnOrAfter(before);
        stats.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void PlayerStats_UpdatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var stats = new PlayerStats();

        // Assert
        var after = DateTime.UtcNow;
        stats.UpdatedAt.Should().BeOnOrAfter(before);
        stats.UpdatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void PlayerStats_CanCalculateTotalFromDifficulties()
    {
        // Arrange
        var stats = new PlayerStats
        {
            Easy = new DifficultyStats { Wins = 5, Losses = 2, Draws = 1, TotalGames = 8 },
            Medium = new DifficultyStats { Wins = 10, Losses = 5, Draws = 3, TotalGames = 18 },
            Hard = new DifficultyStats { Wins = 3, Losses = 10, Draws = 2, TotalGames = 15 }
        };

        // Act - Calculate totals (simulating what the service would do)
        var totalWins = stats.Easy.Wins + stats.Medium.Wins + stats.Hard.Wins;
        var totalLosses = stats.Easy.Losses + stats.Medium.Losses + stats.Hard.Losses;
        var totalDraws = stats.Easy.Draws + stats.Medium.Draws + stats.Hard.Draws;
        var totalGames = stats.Easy.TotalGames + stats.Medium.TotalGames + stats.Hard.TotalGames;

        // Assert
        totalWins.Should().Be(18);
        totalLosses.Should().Be(17);
        totalDraws.Should().Be(6);
        totalGames.Should().Be(41);
    }

    [Theory]
    [InlineData(10, 20, 0.5)]
    [InlineData(0, 10, 0.0)]
    [InlineData(10, 10, 1.0)]
    [InlineData(7, 10, 0.7)]
    public void PlayerStats_WinRateCalculation_IsCorrect(int wins, int totalGames, double expectedRate)
    {
        // Arrange
        var stats = new PlayerStats
        {
            TotalWins = wins,
            TotalGames = totalGames,
            OverallWinRate = totalGames > 0 ? (double)wins / totalGames : 0
        };

        // Assert
        stats.OverallWinRate.Should().BeApproximately(expectedRate, 0.001);
    }

    [Fact]
    public void PlayerStats_WithNegativeValues_StillStoresValues()
    {
        // This tests that the model doesn't enforce constraints (validation is elsewhere)
        // Arrange & Act
        var stats = new PlayerStats
        {
            TotalWins = -5,
            TotalGames = -10,
            OverallWinRate = -0.5
        };

        // Assert - Model stores values as-is (no built-in validation)
        stats.TotalWins.Should().Be(-5);
        stats.TotalGames.Should().Be(-10);
        stats.OverallWinRate.Should().Be(-0.5);
    }

    [Fact]
    public void PlayerStats_MaxWinStreak_AcrossDifficulties()
    {
        // Arrange
        var stats = new PlayerStats
        {
            Easy = new DifficultyStats { WinStreak = 3 },
            Medium = new DifficultyStats { WinStreak = 7 },
            Hard = new DifficultyStats { WinStreak = 5 }
        };

        // Act
        var maxStreak = Math.Max(stats.Easy.WinStreak, Math.Max(stats.Medium.WinStreak, stats.Hard.WinStreak));

        // Assert
        maxStreak.Should().Be(7);
    }

    #endregion
}
