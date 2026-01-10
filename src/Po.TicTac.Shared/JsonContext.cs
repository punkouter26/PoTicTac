using System.Text.Json.Serialization;
using Po.TicTac.Shared.DTOs;
using Po.TicTac.Shared.Models;

namespace Po.TicTac.Shared;

/// <summary>
/// JSON source generator context for AOT-compatible serialization.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PlayerStatsDto))]
[JsonSerializable(typeof(PlayerStats))]
[JsonSerializable(typeof(DifficultyStats))]
[JsonSerializable(typeof(List<PlayerStatsDto>))]
public partial class PoTicTacJsonContext : JsonSerializerContext
{
}
