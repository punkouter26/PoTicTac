import "@typespec/http";
import "@typespec/openapi";
import "@typespec/openapi3";

using TypeSpec.Http;
using TypeSpec.OpenAPI;

@service({
  title: "PoTicTac API",
  version: "1.0.0",
})
@server("https://potictac.azurewebsites.net", "Production")
@server("http://localhost:5000", "Development")
namespace PoTicTac.Api;

@doc("Statistics for a specific difficulty level")
model DifficultyStats {
  @doc("Number of wins at this difficulty")
  wins: int32;

  @doc("Number of losses at this difficulty")
  losses: int32;

  @doc("Number of draws at this difficulty")
  draws: int32;

  @doc("Total games played at this difficulty")
  totalGames: int32;

  @doc("Best win streak achieved")
  winStreak: int32;

  @doc("Current win/loss streak (positive = wins, negative = losses)")
  currentStreak: int32;

  @doc("Average moves per game at this difficulty")
  averageMovesPerGame: float64;

  @doc("Total moves made across all games")
  totalMoves: int32;

  @doc("Win rate as percentage (0-100)")
  @minValue(0)
  @maxValue(100)
  winRate: float64;
}

@doc("Comprehensive player statistics across all difficulty levels")
model PlayerStats {
  @doc("Statistics for Easy difficulty")
  easy: DifficultyStats;

  @doc("Statistics for Medium difficulty")
  medium: DifficultyStats;

  @doc("Statistics for Hard difficulty")
  hard: DifficultyStats;

  @doc("Total wins across all difficulties")
  totalWins: int32;

  @doc("Total losses across all difficulties")
  totalLosses: int32;

  @doc("Total draws across all difficulties")
  totalDraws: int32;

  @doc("Total games played across all difficulties")
  totalGames: int32;

  @doc("Overall win rate as percentage (0-100)")
  @minValue(0)
  @maxValue(100)
  overallWinRate: float64;
}

@doc("Player statistics data transfer object")
model PlayerStatsDto {
  @doc("The player's display name")
  name: string;

  @doc("Comprehensive statistics for the player")
  stats: PlayerStats;
}

@doc("Health check response with dependency status")
model HealthCheckResponse {
  @doc("Overall health status: Healthy, Degraded, or Unhealthy")
  status: "Healthy" | "Degraded" | "Unhealthy";

  @doc("Individual dependency health checks")
  checks: HealthCheckEntry[];

  @doc("Total duration of health check in milliseconds")
  totalDuration: float64;
}

model HealthCheckEntry {
  @doc("Name of the dependency")
  name: string;

  @doc("Health status of this dependency")
  status: "Healthy" | "Degraded" | "Unhealthy";

  @doc("Optional description of the health status")
  description?: string;

  @doc("Duration of this check in milliseconds")
  duration: float64;
}

@route("/api")
@tag("Players")
namespace Players {
  @doc("Retrieves statistics for a specific player")
  @get
  @route("/players/{playerName}/stats")
  op getPlayerStats(@path playerName: string): PlayerStatsDto | NotFoundResponse;

  @doc("Saves or updates statistics for a specific player")
  @put
  @route("/players/{playerName}/stats")
  op savePlayerStats(@path playerName: string, @body stats: PlayerStats): NoContentResponse;
}

@route("/api")
@tag("Statistics")
namespace Statistics {
  @doc("Retrieves all player statistics")
  @get
  @route("/statistics")
  op getAllPlayerStatistics(): PlayerStatsDto[];

  @doc("Retrieves the top players based on win rate")
  @get
  @route("/statistics/leaderboard")
  op getLeaderboard(@query limit?: int32 = 10): PlayerStatsDto[];
}

@route("/api")
@tag("Health")
namespace Health {
  @doc("Health check endpoint - validates connectivity to all external dependencies")
  @get
  @route("/health")
  op healthCheck(): HealthCheckResponse | ServiceUnavailableResponse;
}

@error
model NotFoundResponse {
  @statusCode statusCode: 404;
}

@error
model ServiceUnavailableResponse {
  @statusCode statusCode: 503;
  @body body: HealthCheckResponse;
}

model NoContentResponse {
  @statusCode statusCode: 204;
}
