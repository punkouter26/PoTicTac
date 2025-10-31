using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PoTicTacServer.Models;
using PoTicTacServer.Services;

namespace PoTicTacServer.Controllers;

/// <summary>
/// API endpoints for retrieving and managing player statistics and leaderboards.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly StorageService _storageService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(StorageService storageService, ILogger<StatisticsController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all player statistics.
    /// </summary>
    /// <returns>A list of player names and their comprehensive statistics.</returns>
    /// <response code="200">Returns the list of all players with statistics.</response>
    /// <response code="500">Internal server error occurred while retrieving data.</response>
    /// <remarks>
    /// Returns statistics for all players in the database including:
    /// - Total games played
    /// - Wins, losses, draws
    /// - Win rate percentage
    /// - Current and best winning streaks
    /// - Average moves per game
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PlayerStatsDto>>> GetAllPlayerStatistics()
    {
        _logger.LogInformation("Attempting to retrieve all player statistics.");
        try
        {
            var allPlayers = await _storageService.GetAllPlayersAsync();
            var playerStatsDtos = new List<PlayerStatsDto>();

            foreach (var player in allPlayers)
            {
                playerStatsDtos.Add(new PlayerStatsDto
                {
                    Name = player.Name,
                    Stats = player.Stats
                });
            }
            _logger.LogInformation("Successfully retrieved {Count} player statistics.", playerStatsDtos.Count);
            return Ok(playerStatsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all player statistics.");
            return StatusCode(500, "Internal server error when retrieving statistics.");
        }
    }

    /// <summary>
    /// Retrieves the top players based on win rate.
    /// </summary>
    /// <param name="limit">The maximum number of players to return (default: 10).</param>
    /// <returns>A list of top players ordered by win rate.</returns>
    /// <response code="200">Returns the leaderboard with top players.</response>
    /// <response code="500">Internal server error occurred while retrieving data.</response>
    /// <remarks>
    /// Returns players ranked by win rate in descending order.
    /// Useful for displaying leaderboards and competitive rankings.
    /// </remarks>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(IEnumerable<PlayerStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PlayerStatsDto>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        _logger.LogInformation("Attempting to retrieve leaderboard with limit: {Limit}.", limit);
        try
        {
            var leaderboard = await _storageService.GetLeaderboardAsync(limit);
            var playerStatsDtos = new List<PlayerStatsDto>();

            foreach (var player in leaderboard)
            {
                playerStatsDtos.Add(new PlayerStatsDto
                {
                    Name = player.Name,
                    Stats = player.Stats
                });
            }
            _logger.LogInformation("Successfully retrieved {Count} players for the leaderboard.", playerStatsDtos.Count);
            return Ok(playerStatsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard.");
            return StatusCode(500, "Internal server error when retrieving leaderboard.");
        }
    }

    /// <summary>
    /// Creates sample player data for testing and demonstration purposes.
    /// </summary>
    /// <returns>Confirmation message with number of test players created.</returns>
    /// <response code="200">Test data successfully created.</response>
    /// <response code="500">Internal server error occurred during data creation.</response>
    /// <remarks>
    /// This endpoint creates predefined test data for development and testing:
    /// - Alice: 15 wins, 5 losses, 2 draws
    /// - Bob: 10 wins, 8 losses, 4 draws
    /// - Charlie: 8 wins, 12 losses, 3 draws
    /// - Diana: 20 wins, 3 losses, 1 draw
    /// - Eve: 12 wins, 6 losses, 5 draws
    /// 
    /// **Warning**: This endpoint should be disabled in production environments.
    /// </remarks>
    [HttpPost("test-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateTestData()
    {
        _logger.LogInformation("Creating test player data.");
        try
        {
            var testPlayers = new[]
            {
                new { Name = "Alice", Wins = 15, Losses = 5, Draws = 2 },
                new { Name = "Bob", Wins = 10, Losses = 8, Draws = 4 },
                new { Name = "Charlie", Wins = 8, Losses = 12, Draws = 3 },
                new { Name = "Diana", Wins = 20, Losses = 3, Draws = 1 },
                new { Name = "Eve", Wins = 12, Losses = 6, Draws = 5 }
            };

            foreach (var testPlayer in testPlayers)
            {
                var stats = new PlayerStats
                {
                    Wins = testPlayer.Wins,
                    Losses = testPlayer.Losses,
                    Draws = testPlayer.Draws,
                    TotalGames = testPlayer.Wins + testPlayer.Losses + testPlayer.Draws,
                    WinStreak = testPlayer.Wins > 10 ? 5 : 2,
                    CurrentStreak = 1,
                    AverageMovesPerGame = 7.5
                };
                stats.WinRate = stats.TotalGames > 0 ? (double)stats.Wins / stats.TotalGames : 0;

                await _storageService.SavePlayerStatsAsync(testPlayer.Name, stats);
            }

            _logger.LogInformation("Successfully created test data for {Count} players.", testPlayers.Length);
            return Ok($"Created test data for {testPlayers.Length} players.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test data.");
            return StatusCode(500, "Internal server error when creating test data.");
        }
    }
}

/// <summary>
/// Data transfer object for player statistics.
/// </summary>
public class PlayerStatsDto
{
    /// <summary>
    /// The player's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Comprehensive statistics for the player including wins, losses, draws, streaks, and averages.
    /// </summary>
    public PlayerStats Stats { get; set; } = new PlayerStats();
}
