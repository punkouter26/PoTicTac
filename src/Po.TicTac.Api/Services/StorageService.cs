using System.Text.Json;
using Azure.Data.Tables;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;

namespace Po.TicTac.Api.Services;

public class PlayerStatsEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Players";
    public string RowKey { get; set; } = string.Empty; // PlayerName
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    // Serialize stats as JSON to store complex object
    public string StatsJson { get; set; } = string.Empty;
}

public class StorageService
{
    private readonly TableClient _tableClient;
    private const string TableName = "PlayerStats";

    public StorageService(TableServiceClient tableServiceClient)
    {
        // Get or create the PlayerStats table
        _tableClient = tableServiceClient.GetTableClient(TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<List<PlayerStatsDto>> GetAllPlayerStatsAsync()
    {
        var players = new List<PlayerStatsDto>();

        await foreach (var entity in _tableClient.QueryAsync<PlayerStatsEntity>(filter: $"PartitionKey eq 'Players'"))
        {
            var stats = JsonSerializer.Deserialize<PlayerStats>(entity.StatsJson) ?? new PlayerStats();
            players.Add(new PlayerStatsDto
            {
                Name = entity.RowKey,
                Stats = stats
            });
        }

        return players;
    }

    public async Task<PlayerStats?> GetPlayerStatsByNameAsync(string playerName)
    {
        return await GetPlayerStatsAsync(playerName);
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(string playerName)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<PlayerStatsEntity>("Players", playerName);
            return JsonSerializer.Deserialize<PlayerStats>(response.Value.StatsJson);
        }
        catch
        {
            return null;
        }
    }

    public async Task SavePlayerStatsAsync(string playerName, PlayerStats stats)
    {
        // Sanitize player name for Azure Table RowKey (disallows: / \ # ? and control chars)
        var sanitizedName = SanitizeRowKey(playerName);
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            throw new ArgumentException("Player name cannot be empty or contain only invalid characters", nameof(playerName));
        }

        var entity = new PlayerStatsEntity
        {
            PartitionKey = "Players",
            RowKey = sanitizedName,
            StatsJson = JsonSerializer.Serialize(stats)
        };

        await _tableClient.UpsertEntityAsync(entity);
    }

    /// <summary>
    /// Sanitizes a string for use as Azure Table Storage RowKey.
    /// Removes: / \ # ? and control characters (0x00-0x1F, 0x7F-0x9F)
    /// </summary>
    private static string SanitizeRowKey(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        var sanitized = new System.Text.StringBuilder(input.Length);
        foreach (var c in input)
        {
            // Skip disallowed characters for Azure Table RowKey
            if (c == '/' || c == '\\' || c == '#' || c == '?') continue;
            // Skip control characters
            if (c < 0x20 || (c >= 0x7F && c <= 0x9F)) continue;
            sanitized.Append(c);
        }
        return sanitized.ToString().Trim();
    }

    public async Task<List<(string Name, PlayerStats Stats)>> GetAllPlayersAsync()
    {
        var players = new List<(string Name, PlayerStats Stats)>();

        await foreach (var entity in _tableClient.QueryAsync<PlayerStatsEntity>(filter: $"PartitionKey eq 'Players'"))
        {
            var stats = JsonSerializer.Deserialize<PlayerStats>(entity.StatsJson) ?? new PlayerStats();
            players.Add((entity.RowKey, stats));
        }

        return players;
    }

    public async Task<List<(string Name, PlayerStats Stats)>> GetLeaderboardAsync(int limit)
    {
        var allPlayers = await GetAllPlayersAsync();
        return allPlayers
            .OrderByDescending(p => p.Stats.OverallWinRate)
            .ThenByDescending(p => p.Stats.TotalGames)
            .Take(limit)
            .ToList();
    }
}
