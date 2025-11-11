using MediatR;
using Po.TicTac.Api.Services;
using Po.TicTac.Shared.DTOs;
using Po.TicTac.Shared.Models;

namespace Po.TicTac.Api.Features.Statistics;

/// <summary>
/// Query to retrieve all player statistics
/// </summary>
public record GetAllPlayerStatisticsQuery : IRequest<IEnumerable<PlayerStatsDto>>;

/// <summary>
/// Handler for GetAllPlayerStatisticsQuery
/// </summary>
public class GetAllPlayerStatisticsHandler : IRequestHandler<GetAllPlayerStatisticsQuery, IEnumerable<PlayerStatsDto>>
{
    private readonly StorageService _storageService;
    private readonly ILogger<GetAllPlayerStatisticsHandler> _logger;

    public GetAllPlayerStatisticsHandler(StorageService storageService, ILogger<GetAllPlayerStatisticsHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<IEnumerable<PlayerStatsDto>> Handle(GetAllPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all player statistics");

        try
        {
            var allPlayers = await _storageService.GetAllPlayersAsync();
            var playerStatsDtos = new List<PlayerStatsDto>();

            foreach (var player in allPlayers)
            {
                playerStatsDtos.Add(new PlayerStatsDto
                {
                    Name = player.Name,
                    Stats = player.Stats
                });
            }

            _logger.LogInformation("Successfully retrieved {Count} player statistics", playerStatsDtos.Count);
            return playerStatsDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all player statistics");
            throw;
        }
    }
}

/// <summary>
/// Endpoint configuration for GetAllPlayerStatistics
/// </summary>
public static class GetAllPlayerStatisticsEndpoint
{
    public static IEndpointRouteBuilder MapGetAllPlayerStatistics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/statistics", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllPlayerStatisticsQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllPlayerStatistics")
        .WithTags("Statistics")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Retrieves all player statistics",
            Description = "Returns statistics for all players in the database including wins, losses, draws, win rate, streaks, and averages."
        })
        .Produces<IEnumerable<PlayerStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
