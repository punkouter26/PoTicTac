using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Po.TicTac.Shared;
using Po.TicTac.Shared.DTOs;
using Po.TicTac.Shared.Models;

namespace PoTicTac.Client.Services;

public class StatisticsService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PoTicTacJsonContext.Default
    };

    public StatisticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlayerStatsDto>?> GetAllPlayerStatistics()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync("api/statistics", PoTicTacJsonContext.Default.ListPlayerStatsDto);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error getting all player statistics: {e.Message}");
            return null;
        }
    }

    public async Task<List<PlayerStatsDto>?> GetLeaderboard(int limit = 10)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync($"api/statistics/leaderboard?limit={limit}", PoTicTacJsonContext.Default.ListPlayerStatsDto);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error getting leaderboard: {e.Message}");
            return null;
        }
    }

    public async Task<PlayerStatsDto?> GetPlayerStats(string playerName)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync($"api/players/{playerName}/stats", PoTicTacJsonContext.Default.PlayerStatsDto);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error getting player stats for {playerName}: {e.Message}");
            return null;
        }
    }

    public async Task<bool> SavePlayerStats(string playerName, PlayerStats stats)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/players/{playerName}/stats", stats, PoTicTacJsonContext.Default.PlayerStats);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error saving player stats for {playerName}: {e.Message}");
            return false;
        }
    }
}
