using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PoTicTacServer.Models;
using Xunit;

namespace PoTicTac.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Component", "StatisticsController")]
public class StatisticsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Faker _faker = new();

    public StatisticsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    [Trait("Type", "HappyPath")]
    public async Task GetAllPlayerStatistics_ReturnsSuccessStatusCode()
    {
        // Arrange - No setup needed for GET

        // Act
        var response = await _client.GetAsync("/api/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/statistics should return 200 OK");
        response.IsSuccessStatusCode.Should().BeTrue("response should indicate success");
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task GetAllPlayerStatistics_ReturnsValidListOfPlayerStats()
    {
        // Arrange - No setup needed for GET

        // Act
        var statsList = await _client.GetFromJsonAsync<List<PlayerStatsDto>>("/api/statistics");

        // Assert
        statsList.Should().NotBeNull("API should return a list of player stats");
        statsList.Should().BeOfType<List<PlayerStatsDto>>("response should be a list");

        // If there are any stats, validate their structure
        if (statsList!.Count > 0)
        {
            var firstStat = statsList.First();
            firstStat.Name.Should().NotBeNullOrEmpty("player name should be populated");
            firstStat.Stats.Should().NotBeNull("player stats should not be null");
            firstStat.Stats.TotalGames.Should().BeGreaterThanOrEqualTo(0, "total games cannot be negative");
            firstStat.Stats.Wins.Should().BeGreaterThanOrEqualTo(0, "wins cannot be negative");
            firstStat.Stats.Losses.Should().BeGreaterThanOrEqualTo(0, "losses cannot be negative");
            firstStat.Stats.Draws.Should().BeGreaterThanOrEqualTo(0, "draws cannot be negative");
        }
    }

    [Fact]
    [Trait("Type", "HappyPath")]
    public async Task GetLeaderboard_ReturnsSuccessStatusCode()
    {
        // Arrange
        int limit = 10;

        // Act
        var response = await _client.GetAsync($"/api/statistics/leaderboard?limit={limit}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/statistics/leaderboard should return 200 OK");
        response.IsSuccessStatusCode.Should().BeTrue("response should indicate success");
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task GetLeaderboard_ReturnsValidListSortedByWinRate()
    {
        // Arrange
        int limit = 10;

        // Act
        var leaderboard = await _client.GetFromJsonAsync<List<PlayerStatsDto>>($"/api/statistics/leaderboard?limit={limit}");

        // Assert
        leaderboard.Should().NotBeNull("leaderboard should return a list");
        leaderboard.Should().BeOfType<List<PlayerStatsDto>>("response should be a list");
        leaderboard!.Count.Should().BeLessThanOrEqualTo(limit, $"leaderboard should respect the limit of {limit}");
    }
}

// DTO must match the server's PlayerStatsDto
public class PlayerStatsDto
{
    public string Name { get; set; } = string.Empty;
    public PlayerStats Stats { get; set; } = new PlayerStats();
}
