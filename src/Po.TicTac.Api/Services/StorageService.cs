using System.Text.Json;
using Azure.Data.Tables;
using Po.TicTac.Shared.DTOs;
using Po.TicTac.Shared.Models;

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
        var entity = new PlayerStatsEntity
        {
            PartitionKey = "Players",
            RowKey = playerName,
            StatsJson = JsonSerializer.Serialize(stats)
        };

        await _tableClient.UpsertEntityAsync(entity);
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
