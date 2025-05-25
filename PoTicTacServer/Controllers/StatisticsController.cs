using Microsoft.AspNetCore.Mvc;
using PoTicTacServer.Services;
using PoTicTacServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoTicTacServer.Controllers
{
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
        /// <returns>A list of player names and their statistics.</returns>
        [HttpGet]
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
        /// <param name="limit">The maximum number of players to return.</param>
        /// <returns>A list of top players and their statistics.</returns>
        [HttpGet("leaderboard")]
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
    }

    public class PlayerStatsDto
    {
        public string Name { get; set; } = string.Empty;
        public PlayerStats Stats { get; set; } = new PlayerStats();
    }
}
