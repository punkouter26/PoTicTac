using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using PoTicTacServer.Models;
using System.Threading.Tasks;

namespace PoTicTac.IntegrationTests
{
    public class StatisticsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StatisticsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetStats_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/statistics");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetStats_ReturnsValidPlayerStats()
        {
            var stats = await _client.GetFromJsonAsync<PlayerStats>("/api/statistics");
            Assert.NotNull(stats);
            Assert.True(stats.TotalGames >= 0);
            Assert.True(stats.XWins >= 0);
            Assert.True(stats.OWins >= 0);
            Assert.True(stats.Draws >= 0);
        }

        [Fact]
        public async Task PostStats_UpdatesStatistics()
        {
            var initialStats = await _client.GetFromJsonAsync<PlayerStats>("/api/statistics");

            var newStats = new PlayerStats
            {
                TotalGames = initialStats.TotalGames + 1,
                XWins = initialStats.XWins + 1,
                OWins = initialStats.OWins,
                Draws = initialStats.Draws
            };

            var response = await _client.PostAsJsonAsync("/api/statistics", newStats);
            response.EnsureSuccessStatusCode();

            var updatedStats = await _client.GetFromJsonAsync<PlayerStats>("/api/statistics");
            Assert.Equal(newStats.TotalGames, updatedStats.TotalGames);
            Assert.Equal(newStats.XWins, updatedStats.XWins);
        }
    }
}
