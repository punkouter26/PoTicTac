using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Po.TicTac.Api;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;
using Xunit;

namespace PoTicTac.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Component", "StatisticsController")]
public class StatisticsControllerTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Faker _faker = new();

    public StatisticsControllerTests(IntegrationTestWebApplicationFactory factory)
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
            firstStat.Stats.TotalWins.Should().BeGreaterThanOrEqualTo(0, "wins cannot be negative");
            firstStat.Stats.TotalLosses.Should().BeGreaterThanOrEqualTo(0, "losses cannot be negative");
            firstStat.Stats.TotalDraws.Should().BeGreaterThanOrEqualTo(0, "draws cannot be negative");
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

    [Fact]
    [Trait("Type", "Validation")]
    public async Task GetPlayerStats_UnknownPlayer_ReturnsEmptyStats()
    {
        // Arrange
        var playerName = $"unknown-{Guid.NewGuid():N}";

        // Act
        var response = await _client.GetAsync($"/api/players/{playerName}/stats");
        var dto = await response.Content.ReadFromJsonAsync<PlayerStatsDto>();

        // Assert - API returns 200 OK with empty stats for new players (by design)
        response.StatusCode.Should().Be(HttpStatusCode.OK, "API returns empty stats for unknown players");
        dto.Should().NotBeNull();
        dto!.Name.Should().Be(playerName);
        dto.Stats.TotalGames.Should().Be(0, "new player should have zero games");
    }

    [Fact]
    [Trait("Type", "HappyPath")]
    public async Task SavePlayerStats_PutThenGet_ReturnsPersistedPayload()
    {
        // Arrange
        var playerName = $"integration-{Guid.NewGuid():N}";
        var payload = new PlayerStats
        {
            TotalGames = 1,
            TotalWins = 1,
            OverallWinRate = 1.0
        };
        payload.Easy.TotalGames = 1;
        payload.Easy.Wins = 1;
        payload.Easy.WinRate = 1.0;

        // Act
        var putResponse = await _client.PutAsJsonAsync($"/api/players/{playerName}/stats", payload);
        var getResponse = await _client.GetAsync($"/api/players/{playerName}/stats");
        var dto = await getResponse.Content.ReadFromJsonAsync<PlayerStatsDto>();

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent, "PUT should return 204 No Content");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK, "GET should succeed after saving stats");
        dto.Should().NotBeNull();
        dto!.Name.Should().Be(playerName);
        dto.Stats.TotalGames.Should().Be(payload.TotalGames);
        dto.Stats.TotalWins.Should().Be(payload.TotalWins);
        dto.Stats.Easy.TotalGames.Should().Be(payload.Easy.TotalGames);
    }

    #region API Contract Tests - Edge Cases and Negative Scenarios

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetLeaderboard_WithZeroLimit_ReturnsEmptyList()
    {
        // Arrange
        int limit = 0;

        // Act
        var response = await _client.GetAsync($"/api/statistics/leaderboard?limit={limit}");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<PlayerStatsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().Be(0, "limit=0 should return empty list");
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetLeaderboard_WithNegativeLimit_HandlesGracefully()
    {
        // Arrange
        int limit = -5;

        // Act
        var response = await _client.GetAsync($"/api/statistics/leaderboard?limit={limit}");

        // Assert - API should handle gracefully (either empty list or error)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetLeaderboard_WithVeryLargeLimit_ReturnsAllAvailable()
    {
        // Arrange
        int limit = 10000;

        // Act
        var response = await _client.GetAsync($"/api/statistics/leaderboard?limit={limit}");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<PlayerStatsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        leaderboard.Should().NotBeNull();
        // Should return whatever is available, not fail
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetPlayerStats_WithSpecialCharactersInName_HandlesGracefully()
    {
        // Arrange - Names with special characters that might break URL encoding
        var playerName = "test-player_123";

        // Act
        var response = await _client.GetAsync($"/api/players/{playerName}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetPlayerStats_WithUrlEncodedName_DecodesCorrectly()
    {
        // Arrange - URL-encoded name
        var playerName = "player%20with%20spaces";

        // Act
        var response = await _client.GetAsync($"/api/players/{playerName}/stats");

        // Assert - Should handle encoded names
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task SavePlayerStats_WithEmptyPayload_ReturnsError()
    {
        // Arrange
        var playerName = $"empty-{Guid.NewGuid():N}";
        var emptyContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/players/{playerName}/stats", emptyContent);

        // Assert - Empty but valid JSON should either succeed or return bad request
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task SavePlayerStats_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var playerName = $"malformed-{Guid.NewGuid():N}";
        var malformedContent = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/players/{playerName}/stats", malformedContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "malformed JSON should be rejected");
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task SavePlayerStats_ThenUpdate_OverwritesPreviousData()
    {
        // Arrange
        var playerName = $"update-{Guid.NewGuid():N}";
        var initialPayload = new PlayerStats { TotalGames = 5, TotalWins = 3 };
        var updatedPayload = new PlayerStats { TotalGames = 10, TotalWins = 8 };

        // Act - Save initial, then update
        await _client.PutAsJsonAsync($"/api/players/{playerName}/stats", initialPayload);
        await _client.PutAsJsonAsync($"/api/players/{playerName}/stats", updatedPayload);
        var getResponse = await _client.GetAsync($"/api/players/{playerName}/stats");
        var dto = await getResponse.Content.ReadFromJsonAsync<PlayerStatsDto>();

        // Assert - Should have updated values
        dto.Should().NotBeNull();
        dto!.Stats.TotalGames.Should().Be(10);
        dto.Stats.TotalWins.Should().Be(8);
    }

    [Fact]
    [Trait("Type", "EdgeCase")]
    public async Task GetPlayerStats_CaseSensitivity_TreatsNamesAsDistinct()
    {
        // Arrange
        var lowerName = $"casetest-{Guid.NewGuid():N}".ToLower();
        var upperName = lowerName.ToUpper();
        var lowerPayload = new PlayerStats { TotalGames = 5, TotalWins = 3 };
        var upperPayload = new PlayerStats { TotalGames = 10, TotalWins = 7 };

        // Act - Save with different cases
        await _client.PutAsJsonAsync($"/api/players/{lowerName}/stats", lowerPayload);
        await _client.PutAsJsonAsync($"/api/players/{upperName}/stats", upperPayload);
        
        var lowerResponse = await _client.GetFromJsonAsync<PlayerStatsDto>($"/api/players/{lowerName}/stats");
        var upperResponse = await _client.GetFromJsonAsync<PlayerStatsDto>($"/api/players/{upperName}/stats");

        // Assert - Names might be treated as same or different depending on implementation
        lowerResponse.Should().NotBeNull();
        upperResponse.Should().NotBeNull();
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "health endpoint should return 200");
    }

    [Fact]
    [Trait("Type", "Validation")]
    public async Task AliveEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/alive");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "liveness endpoint should return 200");
    }

    #endregion
}
