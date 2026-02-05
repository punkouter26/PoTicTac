using MediatR;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;

namespace Po.TicTac.Api.Features.Statistics;

/// <summary>
/// Query to retrieve all player statistics
/// </summary>
public record GetAllPlayerStatisticsQuery : IRequest<IEnumerable<PlayerStatsDto>>;

/// <summary>
/// Handler for GetAllPlayerStatisticsQuery using primary constructor (C# 14).
/// </summary>
public sealed class GetAllPlayerStatisticsHandler(
    StorageService storageService,
    ILogger<GetAllPlayerStatisticsHandler> logger) : IRequestHandler<GetAllPlayerStatisticsQuery, IEnumerable<PlayerStatsDto>>
{
    public async Task<IEnumerable<PlayerStatsDto>> Handle(GetAllPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all player statistics");

        try
        {
            var allPlayers = await storageService.GetAllPlayersAsync();
            List<PlayerStatsDto> playerStatsDtos = [.. allPlayers.Select(p => new PlayerStatsDto { Name = p.Name, Stats = p.Stats })];

            logger.LogInformation("Successfully retrieved {Count} player statistics", playerStatsDtos.Count);
            return playerStatsDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all player statistics");
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
        .WithSummary("Retrieves all player statistics")
        .WithDescription("Returns statistics for all players in the database including wins, losses, draws, win rate, streaks, and averages.")
        .Produces<IEnumerable<PlayerStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
