using Po.TicTac.Api.Models;

namespace Po.TicTac.Api.DTOs;

/// <summary>
/// Data transfer object for player statistics.
/// </summary>
public sealed class PlayerStatsDto
{
    public required string Name { get; init; }
    public required PlayerStats Stats { get; init; }
}
