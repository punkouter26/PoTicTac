using Microsoft.AspNetCore.Mvc;
using PoTicTacServer.Models;
using PoTicTacServer.Services;

namespace PoTicTacServer.Controllers;

/// <summary>
/// API endpoints for managing player data and statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly StorageService _storageService;

    public PlayersController(StorageService storageService)
    {
        _storageService = storageService;
    }

    /// <summary>
    /// Retrieves statistics for a specific player by name.
    /// </summary>
    /// <param name="playerName">The name of the player to retrieve statistics for.</param>
    /// <returns>The player's statistics including wins, losses, draws, win rate, and streaks.</returns>
    /// <response code="200">Returns the player's statistics.</response>
    /// <response code="404">Player not found in the database.</response>
    [HttpGet("{playerName}")]
    [ProducesResponseType(typeof(PlayerStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerStats>> GetPlayerStats(string playerName)
    {
        var stats = await _storageService.GetPlayerStatsAsync(playerName);
        if (stats == null)
        {
            return NotFound();
        }
        return stats;
    }

    /// <summary>
    /// Saves or updates statistics for a specific player.
    /// </summary>
    /// <param name="playerName">The name of the player to save statistics for.</param>
    /// <param name="stats">The statistics data to save.</param>
    /// <returns>Success indication.</returns>
    /// <response code="200">Statistics successfully saved.</response>
    [HttpPut("{playerName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SavePlayerStats(string playerName, PlayerStats stats)
    {
        await _storageService.SavePlayerStatsAsync(playerName, stats);
        return Ok();
    }

    /// <summary>
    /// Retrieves all players and their statistics from the database.
    /// </summary>
    /// <returns>A list of all players with their names and statistics.</returns>
    /// <response code="200">Returns all players and their statistics.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllPlayers()
    {
        var players = await _storageService.GetAllPlayersAsync();
        return Ok(players.Select(p => new { name = p.Name, stats = p.Stats }));
    }

    /// <summary>
    /// Retrieves the top players ranked by win rate.
    /// </summary>
    /// <param name="limit">The maximum number of players to return (default: 10).</param>
    /// <returns>A list of top players ordered by win rate.</returns>
    /// <response code="200">Returns the leaderboard with top players.</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        var players = await _storageService.GetLeaderboardAsync(limit);
        return Ok(players.Select(p => new { name = p.Name, stats = p.Stats }));
    }
}
