# PoTicTac API Contracts

This folder contains **TypeSpec** API contracts that define the PoTicTac REST API.

## What is TypeSpec?

TypeSpec is a language for defining APIs that generates OpenAPI, JSON Schema, and client SDKs. It provides:
- Single source of truth for API contracts
- Type-safe API definitions
- Auto-generated OpenAPI specs

## Quick Start

```bash
# Install dependencies
npm install

# Compile TypeSpec to OpenAPI
npm run build

# Watch for changes
npm run watch
```

## Files

| File | Purpose |
|------|---------|
| `main.tsp` | API contract definitions |
| `tspconfig.yaml` | TypeSpec compiler configuration |
| `openapi.yaml` | Generated OpenAPI 3.0 spec (after build) |

## API Endpoints

### Players
- `GET /api/players/{playerName}/stats` - Get player statistics
- `PUT /api/players/{playerName}/stats` - Save/update player statistics

### Statistics
- `GET /api/statistics` - Get all player statistics
- `GET /api/statistics/leaderboard?limit=10` - Get top players by win rate

### Health
- `GET /api/health` - Health check with dependency status

## Integration with .NET API

The TypeSpec contracts serve as the authoritative API specification. The .NET Minimal API endpoints in `src/Po.TicTac.Api/Features/` should match these contracts.

To validate alignment:
1. Run `npm run build` to generate `openapi.yaml`
2. Compare against the API's runtime OpenAPI at `/openapi/v1.json`
