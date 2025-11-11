using MediatR;
using Po.TicTac.Api.Services;
using Po.TicTac.Shared.DTOs;

namespace Po.TicTac.Api.Features.Statistics;

/// <summary>
/// Query to retrieve leaderboard
/// </summary>
public record GetLeaderboardQuery(int Limit = 10) : IRequest<IEnumerable<PlayerStatsDto>>;

/// <summary>
/// Handler for GetLeaderboardQuery
/// </summary>
public class GetLeaderboardHandler : IRequestHandler<GetLeaderboardQuery, IEnumerable<PlayerStatsDto>>
{
    private readonly StorageService _storageService;
    private readonly ILogger<GetLeaderboardHandler> _logger;

    public GetLeaderboardHandler(StorageService storageService, ILogger<GetLeaderboardHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<IEnumerable<PlayerStatsDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving leaderboard with limit: {Limit}", request.Limit);

        try
        {
            var leaderboard = await _storageService.GetLeaderboardAsync(request.Limit);
            var playerStatsDtos = new List<PlayerStatsDto>();

            foreach (var player in leaderboard)
            {
                playerStatsDtos.Add(new PlayerStatsDto
                {
                    Name = player.Name,
                    Stats = player.Stats
                });
            }

            _logger.LogInformation("Successfully retrieved {Count} players for the leaderboard", playerStatsDtos.Count);
            return playerStatsDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard");
            throw;
        }
    }
}

/// <summary>
/// Endpoint configuration for GetLeaderboard
/// </summary>
public static class GetLeaderboardEndpoint
{
    public static IEndpointRouteBuilder MapGetLeaderboard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/statistics/leaderboard", async (IMediator mediator, int limit = 10) =>
        {
            var result = await mediator.Send(new GetLeaderboardQuery(limit));
            return Results.Ok(result);
        })
        .WithName("GetLeaderboard")
        .WithTags("Statistics")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Retrieves the top players based on win rate",
            Description = "Returns players ranked by win rate in descending order. Useful for displaying leaderboards and competitive rankings."
        })
        .Produces<IEnumerable<PlayerStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
