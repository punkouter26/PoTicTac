using Azure.Data.Tables;
using System.Text.Json;
using PoTicTacServer.Models;
using Microsoft.Extensions.Logging;

namespace PoTicTacServer.Services;

public class StorageService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<StorageService> _logger;
    private const string TableName = "PlayerStats";

    public StorageService(IConfiguration configuration, ILogger<StorageService> logger)
    {
        _logger = logger;
        
        try
        {
            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            _logger.LogInformation("Created TableServiceClient for development storage");
            
            _tableClient = tableServiceClient.GetTableClient(TableName);
            _logger.LogInformation("Created TableClient for table: {TableName}", TableName);
            
            _tableClient.CreateIfNotExists();
            _logger.LogInformation("Successfully ensured table {TableName} exists", TableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing table storage for table {TableName}", TableName);
            throw;
        }
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(string playerName)
    {
        try
        {
            var entity = await _tableClient.GetEntityAsync<PlayerEntity>(
                partitionKey: "players",
                rowKey: playerName.ToLowerInvariant());

            return JsonSerializer.Deserialize<PlayerStats>(entity.Value.Stats);
        }
        catch (Azure.RequestFailedException)
        {
            return null;
        }
    }

    public async Task SavePlayerStatsAsync(string playerName, PlayerStats stats)
    {
        var entity = new PlayerEntity
        {
            RowKey = playerName.ToLowerInvariant(),
            Name = playerName,
            Stats = JsonSerializer.Serialize(stats),
            LastUpdated = DateTime.UtcNow.ToString("o")
        };

        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task<List<(string Name, PlayerStats Stats)>> GetAllPlayersAsync()
    {
        var players = new List<(string Name, PlayerStats Stats)>();
        var entities = _tableClient.QueryAsync<PlayerEntity>();

        await foreach (var entity in entities)
        {
            var stats = JsonSerializer.Deserialize<PlayerStats>(entity.Stats);
            if (stats != null)
            {
                players.Add((entity.Name, stats));
            }
        }

        return players;
    }

    public async Task<List<(string Name, PlayerStats Stats)>> GetLeaderboardAsync(int limit)
    {
        var players = await GetAllPlayersAsync();
        return players
            .OrderByDescending(p => p.Stats.WinRate)
            .Take(limit)
            .ToList();
    }
} 