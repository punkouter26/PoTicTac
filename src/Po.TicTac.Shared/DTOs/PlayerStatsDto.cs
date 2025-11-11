using Po.TicTac.Shared.Models;

namespace Po.TicTac.Shared.DTOs;

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
