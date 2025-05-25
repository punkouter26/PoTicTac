using Microsoft.AspNetCore.Mvc;
using PoTicTacServer.Models;
using PoTicTacServer.Services;

namespace PoTicTacServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly StorageService _storageService;

    public PlayersController(StorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("{playerName}")]
    public async Task<ActionResult<PlayerStats>> GetPlayerStats(string playerName)
    {
        var stats = await _storageService.GetPlayerStatsAsync(playerName);
        if (stats == null)
        {
            return NotFound();
        }
        return stats;
    }

    [HttpPut("{playerName}")]
    public async Task<IActionResult> SavePlayerStats(string playerName, PlayerStats stats)
    {
        await _storageService.SavePlayerStatsAsync(playerName, stats);
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllPlayers()
    {
        var players = await _storageService.GetAllPlayersAsync();
        return Ok(players.Select(p => new { name = p.Name, stats = p.Stats }));
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<object>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        var players = await _storageService.GetLeaderboardAsync(limit);
        return Ok(players.Select(p => new { name = p.Name, stats = p.Stats }));
    }
} 