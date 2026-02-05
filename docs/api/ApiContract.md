# PoTicTac API Contract

## Overview

The PoTicTac API provides RESTful endpoints for player statistics management, leaderboards, and health monitoring. The API uses .NET 10 Minimal APIs with OpenAPI documentation.

**Base URL**: 
- Local: `http://localhost:5000/api`
- Production: `https://api.braveground-e6b1356c.eastus.azurecontainerapps.io/api`

**Documentation**: `/swagger` (Swagger UI)

---

## Authentication

Currently **none required**. Player identification is by name only.

*Future: OAuth 2.0 with Azure Entra ID planned*

---

## Endpoints

### Health Check

#### GET /api/health

Returns the health status of the API and its dependencies.

**Response**: `200 OK`

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "AzureTableStorage": {
      "status": "Healthy",
      "duration": "00:00:00.0500000"
    }
  }
}
```

---

### Statistics

#### GET /api/statistics

Retrieves all player statistics.

**Response**: `200 OK`

```json
[
  {
    "name": "PlayerOne",
    "stats": {
      "playerId": "",
      "playerName": "",
      "easy": {
        "wins": 5,
        "losses": 2,
        "draws": 1,
        "totalGames": 8,
        "winStreak": 3,
        "currentStreak": 1,
        "averageMovesPerGame": 4.5,
        "totalMoves": 36,
        "winRate": 0.625
      },
      "medium": { ... },
      "hard": { ... },
      "totalWins": 10,
      "totalLosses": 5,
      "totalDraws": 2,
      "totalGames": 17,
      "winRate": 0.588,
      "overallWinRate": 0.588,
      "createdAt": "2026-01-15T10:30:00Z",
      "updatedAt": "2026-02-05T14:22:00Z"
    }
  }
]
```

---

#### GET /api/statistics/leaderboard

Retrieves the top players ranked by win rate.

**Query Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `limit` | int | 10 | Maximum players to return |

**Response**: `200 OK`

```json
[
  {
    "name": "Champion",
    "stats": {
      "totalWins": 50,
      "totalGames": 60,
      "overallWinRate": 0.833,
      ...
    }
  },
  {
    "name": "SecondPlace",
    "stats": { ... }
  }
]
```

---

### Players

#### GET /api/players/{playerName}/stats

Retrieves statistics for a specific player.

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `playerName` | string | URL-encoded player name |

**Response**: `200 OK`

```json
{
  "name": "PlayerOne",
  "stats": {
    "easy": { ... },
    "medium": { ... },
    "hard": { ... },
    "totalWins": 10,
    "totalLosses": 5,
    "totalDraws": 2,
    "totalGames": 17,
    "overallWinRate": 0.588
  }
}
```

**Response**: `404 Not Found` (if player doesn't exist)

---

#### PUT /api/players/{playerName}/stats

Creates or updates player statistics.

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `playerName` | string | URL-encoded player name |

**Request Body**:

```json
{
  "totalWins": 11,
  "totalLosses": 5,
  "totalDraws": 2,
  "totalGames": 18,
  "overallWinRate": 0.611,
  "easy": {
    "wins": 6,
    "losses": 2,
    "draws": 1,
    "totalGames": 9,
    "winStreak": 4,
    "currentStreak": 2,
    "averageMovesPerGame": 4.2,
    "totalMoves": 38,
    "winRate": 0.667
  },
  "medium": {
    "wins": 0,
    "losses": 0,
    "draws": 0,
    "totalGames": 0,
    "winStreak": 0,
    "currentStreak": 0,
    "averageMovesPerGame": 0,
    "totalMoves": 0,
    "winRate": 0
  },
  "hard": { ... }
}
```

**Response**: `204 No Content`

---

## Data Types

### PlayerStatsDto

```typescript
interface PlayerStatsDto {
  name: string;
  stats: PlayerStats;
}
```

### PlayerStats

```typescript
interface PlayerStats {
  playerId: string;
  playerName: string;
  easy: DifficultyStats;
  medium: DifficultyStats;
  hard: DifficultyStats;
  totalWins: number;
  totalLosses: number;
  totalDraws: number;
  totalGames: number;
  winRate: number;
  overallWinRate: number;
  createdAt: string; // ISO 8601
  updatedAt: string; // ISO 8601
}
```

### DifficultyStats

```typescript
interface DifficultyStats {
  wins: number;
  losses: number;
  draws: number;
  totalGames: number;
  winStreak: number;
  currentStreak: number;
  averageMovesPerGame: number;
  totalMoves: number;
  winRate: number;
}
```

---

## Error Responses

### Standard Error Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Player name cannot be empty"
}
```

### HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET |
| 204 | No Content | Successful PUT/DELETE |
| 400 | Bad Request | Invalid input |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server error |

---

## Rate Limiting

Currently **no rate limiting** is implemented.

*Future: Consider 100 requests/minute per IP*

---

## CORS Configuration

Allowed origins (configurable via `appsettings.json`):
- `http://localhost:3000`
- `https://localhost:3000`
- Production frontend URL

---

## Testing

Use the `.http` file at `src/Po.TicTac.Api/api-tests.http` with VS Code REST Client extension for interactive testing.

---

## Related Documentation

- [Swagger UI](/swagger) - Interactive API documentation
- [DataWorkflow.mmd](../DataWorkflow.mmd) - Data flow diagrams
- [DatabaseSchema.mmd](../DatabaseSchema.mmd) - Storage schema
