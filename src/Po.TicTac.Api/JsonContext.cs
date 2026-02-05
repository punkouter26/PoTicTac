using System.Text.Json.Serialization;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;

namespace Po.TicTac.Api;

/// <summary>
/// JSON source generator context for AOT-compatible serialization.
/// Hardened with strict serialization options.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    AllowTrailingCommas = false,
    WriteIndented = false)]
[JsonSerializable(typeof(PlayerStatsDto))]
[JsonSerializable(typeof(PlayerStats))]
[JsonSerializable(typeof(DifficultyStats))]
[JsonSerializable(typeof(List<PlayerStatsDto>))]
[JsonSerializable(typeof(StatsSummaryModel))]
public partial class PoTicTacJsonContext : JsonSerializerContext
{
}
