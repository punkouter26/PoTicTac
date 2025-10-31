using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoTicTac.Client.Models; // Assuming PlayerStatsDto will be defined here or similar

namespace PoTicTac.Client.Services;

public class StatisticsService
{
    private readonly HttpClient _httpClient;

    public StatisticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlayerStatsDto>?> GetAllPlayerStatistics()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<PlayerStatsDto>>("api/Statistics");
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
            return await _httpClient.GetFromJsonAsync<List<PlayerStatsDto>>($"api/Statistics/leaderboard?limit={limit}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error getting leaderboard: {e.Message}");
            return null;
        }
    }
}

// Define PlayerStatsDto for the client-side, mirroring the server-side DTO
public class PlayerStatsDto
{
    public string Name { get; set; } = string.Empty;
    public PlayerStats Stats { get; set; } = new PlayerStats();
}
