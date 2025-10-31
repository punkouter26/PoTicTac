# API Testing Quick Reference

## Using the .http File

The project includes `PoTicTacServer/api-tests.http` which contains comprehensive API endpoint tests.

### Prerequisites

1. **Install REST Client Extension** (VS Code):
   - Open VS Code Extensions (Ctrl+Shift+X)
   - Search for "REST Client"
   - Install the extension by Huachao Mao

2. **Start the API Server**:
   ```powershell
   dotnet run --project PoTicTacServer
   ```
   Or use the VS Code task: `Ctrl+Shift+P` → "Tasks: Run Task" → "start-server"

### Running Tests

1. Open `PoTicTacServer/api-tests.http`
2. Click "Send Request" above any test
3. View the response in the right panel
4. Inline assertions automatically validate responses

### Test Organization

#### Health & Diagnostics
- Test 1: Health Check (200 OK)
- Test 15: Health Performance (<1s)

#### Players CRUD
- Test 2: Get All Players
- Test 3: Get Leaderboard (sorted by wins)
- Test 4: Get Player by ID
- Test 5: Create Player (201 Created)
- Test 6: Update Player (200 OK)
- Test 7: Record Win
- Test 8: Record Loss
- Test 9: Record Draw
- Test 10: Delete Player (204 No Content)
- Test 11: Verify Delete (404 Not Found)

#### Error Handling
- Test 12: Invalid Player ID (404)
- Test 13: Invalid Request Body (400)
- Test 14: Malformed JSON (400)

#### Security & Performance
- Test 16: Get All Players Performance (<2s)
- Test 17: OPTIONS Request (CORS preflight)
- Test 18: Security Headers Validation

#### Batch Operations
- Test 19: Create Multiple Players
- Test 20: Leaderboard Contains New Player

### Variables

The .http file uses variables for dynamic testing:

```http
@hostname = localhost
@port = 5000
@host = {{hostname}}:{{port}}
@contentType = application/json
```

Chained requests capture response data:
```http
# Create player
@newPlayerId = {{createPlayer.response.body.id}}

# Use player ID in subsequent requests
GET http://{{host}}/api/players/{{newPlayerId}}
```

### Inline Assertions

Each test includes automatic validation:

```http
###
# @assert {{response.status}} == 200
# @assert {{response.headers.content-type}} includes application/json
# @assert {{response.body.status}} == "Healthy"
```

### Common Workflows

#### 1. Create and Test Player
```http
# 1. Create player
POST http://{{host}}/api/players
Content-Type: application/json

{
  "id": "test-player-123",
  "name": "Test Player",
  "stats": { "wins": 0, "losses": 0, "draws": 0, "totalGames": 0 }
}

# 2. Record a win
POST http://{{host}}/api/players/test-player-123/win

# 3. Verify stats updated
GET http://{{host}}/api/players/test-player-123

# 4. Cleanup
DELETE http://{{host}}/api/players/test-player-123
```

#### 2. Verify Leaderboard Sorting
```http
# Get leaderboard
GET http://{{host}}/api/players/leaderboard

# Expected: Players sorted by wins (descending)
```

#### 3. Error Handling Validation
```http
# Try to get non-existent player
GET http://{{host}}/api/players/does-not-exist

# Expected: 404 Not Found with Problem Details JSON
```

### Tips

1. **Run All Tests**: Click "Send Request" on each test sequentially
2. **Random Data**: Use `{{$randomInt}}` for unique player IDs
3. **Environment Switching**: Change `@hostname` and `@port` for different environments
4. **Response Inspection**: Click JSON response to format and inspect
5. **Copy as cURL**: Right-click request → "Copy Request As cURL"

### Troubleshooting

**"Connection refused"**
- Ensure API server is running on port 5000
- Check firewall settings
- Verify `launchSettings.json` port configuration

**"404 Not Found" on valid endpoint**
- Check API server console for routing errors
- Verify controller route attributes
- Ensure endpoint is registered in Program.cs

**Assertion failures**
- Read the assertion message for details
- Compare expected vs actual in response panel
- Check if API behavior changed

### CI/CD Integration

The .http file can be executed in CI pipelines using tools like:
- **httpyac**: CLI runner for .http files
- **REST Client CLI**: Command-line execution
- **newman**: If converted to Postman collection

Example with httpyac:
```powershell
# Install httpyac
npm install -g httpyac

# Run tests
httpyac send PoTicTacServer/api-tests.http --all
```

### Related Documentation

- [Testing Documentation](../docs/TESTING.md)
- [Phase 3 Progress](../docs/PHASE3_PROGRESS.md)
- [API Controllers](../PoTicTacServer/Controllers/)
- [Swagger/OpenAPI](http://localhost:5000/swagger) (when server running)
