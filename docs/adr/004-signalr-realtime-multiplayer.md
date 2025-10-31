# ADR-004: Use SignalR for Real-Time Multiplayer

## Status
**Accepted** - August 2, 2025

## Context

We needed a real-time communication technology to enable live multiplayer gameplay in PoTicTac. The requirements were:

- **Bidirectional Communication**: Server can push updates to clients and vice versa
- **Low Latency**: Sub-second response times for move synchronization
- **Automatic Reconnection**: Handle network interruptions gracefully
- **Connection Management**: Track active players and game sessions
- **Cross-Platform**: Work across browsers, mobile devices, and desktop
- **Scalability**: Support multiple concurrent game sessions
- **Simplicity**: Minimal boilerplate code for real-time features
- **ASP.NET Core Integration**: Native support for .NET ecosystem

Traditional HTTP polling or long-polling introduces latency and server overhead. WebSocket-based protocols enable instant bidirectional communication.

## Decision

We will use **SignalR** (ASP.NET Core) for real-time multiplayer communication.

### Architecture

**Server-Side**:
```csharp
public class GameHub : Hub
{
    // Clients call these methods
    public async Task CreateGame(string playerName);
    public async Task JoinGame(string gameId, string playerName);
    public async Task MakeMove(string gameId, int row, int col);
    public async Task LeaveGame(string gameId);
    
    // Hub broadcasts to clients
    await Clients.Group(gameId).SendAsync("OpponentJoined", playerName);
    await Clients.Group(gameId).SendAsync("MoveMade", row, col, playerName);
    await Clients.Group(gameId).SendAsync("GameOver", winner);
}
```

**Client-Side** (Blazor):
```csharp
public class SignalRService
{
    private HubConnection _hubConnection;
    
    public async Task ConnectAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("/gamehub")
            .WithAutomaticReconnect()
            .Build();
            
        _hubConnection.On<string>("OpponentJoined", OnOpponentJoined);
        _hubConnection.On<int, int, string>("MoveMade", OnMoveMade);
        
        await _hubConnection.StartAsync();
    }
}
```

### Protocol Selection

SignalR uses **WebSockets** as the preferred transport, with automatic fallback to:
1. **WebSockets** (preferred, bidirectional)
2. **Server-Sent Events** (SSE) - if WebSockets blocked
3. **Long Polling** - if both above unavailable

## Consequences

### Positive

✅ **Low Latency**: WebSocket protocol provides <50ms round-trip for move synchronization  
✅ **Automatic Reconnection**: Built-in reconnection logic with exponential backoff  
✅ **Strongly Typed**: Compile-time type safety for messages (unlike raw WebSockets)  
✅ **Connection Management**: Automatic tracking of connected clients  
✅ **Groups**: Easy broadcasting to specific game rooms  
✅ **ASP.NET Core Integration**: Shares authentication, DI, middleware pipeline  
✅ **Scalability**: Redis backplane for multi-server deployments (future enhancement)  
✅ **Browser Compatibility**: Automatic transport fallback for older browsers  
✅ **Minimal Code**: High-level abstractions hide WebSocket complexity  

### Negative

⚠️ **Stateful Connections**: Requires sticky sessions in multi-instance deployments (or Redis backplane)  
⚠️ **Connection Overhead**: Each client maintains persistent connection (more server resources than HTTP)  
⚠️ **Firewall/Proxy Issues**: Some corporate firewalls block WebSockets (mitigated by fallback transports)  
⚠️ **Debugging Complexity**: Harder to debug than HTTP (requires browser dev tools WebSocket inspector)  
⚠️ **Scalability Ceiling**: Single server limited to ~10,000 concurrent connections (future problem)  

### Trade-offs

- **Server Resources vs. Latency**: Persistent connections use more memory for instant communication
- **Complexity vs. Real-Time UX**: More complex than HTTP for dramatically better user experience
- **Sticky Sessions vs. Simplicity**: Single-server deployment is simple; multi-server requires Redis backplane

## Alternatives Considered

### 1. HTTP Polling
**Pros**: Simple, stateless, works everywhere, no special firewall rules  
**Cons**: **High latency** (1-5 second delays), excessive network traffic, poor UX, server load from constant polling  
**Why Rejected**: Unacceptable latency for real-time game; opponent moves would appear with noticeable delays

### 2. Long Polling (Comet)
**Pros**: Better than short polling, works on old browsers, no WebSocket support needed  
**Cons**: Still has latency (500ms-2s), complex to implement, server resources for held connections, not truly bidirectional  
**Why Rejected**: SignalR already provides long polling as a fallback transport; no reason to implement manually

### 3. Server-Sent Events (SSE)
**Pros**: Native browser API, simpler than WebSockets, automatic reconnection  
**Cons**: **Unidirectional** (server → client only), requires separate HTTP requests for client → server, connection limit (6 per domain)  
**Why Rejected**: One-way communication insufficient; would need HTTP requests for player moves (hybrid approach is less clean)

### 4. Raw WebSockets (without SignalR)
**Pros**: Full control, no framework overhead, slightly less bandwidth  
**Cons**: **No automatic reconnection**, manual JSON serialization, no groups/broadcasting, no fallback transports, more code  
**Why Rejected**: SignalR provides all the features we need with less code; raw WebSockets would require building our own protocol

### 5. Socket.IO (JavaScript library)
**Pros**: Very popular in JavaScript ecosystem, excellent browser support, automatic reconnection, rooms/namespaces  
**Cons**: **JavaScript-only** (no C# client), requires Node.js backend or integration layer, separate tech stack  
**Why Rejected**: Incompatible with Blazor WebAssembly (C# client); would require separate JavaScript backend or translation layer

### 6. gRPC with Streaming
**Pros**: High performance, bidirectional streaming, strongly typed, HTTP/2  
**Cons**: **Poor browser support** (requires gRPC-Web proxy), more complex than SignalR, overkill for simple game state  
**Why Rejected**: Designed for microservice communication, not browser clients; SignalR is more appropriate for web apps

### 7. Firebase Realtime Database
**Pros**: Fully managed, real-time sync, offline support, free tier  
**Cons**: **External dependency**, NoSQL schema required, vendor lock-in, not Azure-native, JavaScript-centric  
**Why Rejected**: Adds external SaaS dependency outside Azure ecosystem; we already have Azure Table Storage for persistence

## Implementation Notes

### Game Session Management

```csharp
// In-memory game state (single server)
private static ConcurrentDictionary<string, GameState> _games = new();
private static ConcurrentDictionary<string, string> _userGameMap = new();

public async Task CreateGame(string playerName)
{
    var gameId = Guid.NewGuid().ToString();
    _games[gameId] = new GameState { Player1 = playerName };
    _userGameMap[Context.ConnectionId] = gameId;
    
    await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    await Clients.Caller.SendAsync("GameCreated", gameId);
}

public async Task JoinGame(string gameId, string playerName)
{
    if (_games.TryGetValue(gameId, out var game) && game.Player2 == null)
    {
        game.Player2 = playerName;
        _userGameMap[Context.ConnectionId] = gameId;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).SendAsync("OpponentJoined", playerName);
        await Clients.Group(gameId).SendAsync("GameStarted", game);
    }
}
```

### Automatic Reconnection

```csharp
// Client-side configuration
_hubConnection = new HubConnectionBuilder()
    .WithUrl("/gamehub")
    .WithAutomaticReconnect(new[] {
        TimeSpan.FromSeconds(0),  // Immediate retry
        TimeSpan.FromSeconds(2),  // 2 second delay
        TimeSpan.FromSeconds(10), // 10 second delay
        TimeSpan.FromSeconds(30)  // Max 30 second delay
    })
    .Build();

_hubConnection.Reconnecting += error =>
{
    Console.WriteLine("Connection lost, reconnecting...");
    return Task.CompletedTask;
};

_hubConnection.Reconnected += connectionId =>
{
    Console.WriteLine("Reconnected successfully!");
    // Re-join game if in progress
    return Task.CompletedTask;
};
```

### Scalability Considerations

**Current (Single Server)**:
- In-memory `ConcurrentDictionary` for game state
- Works for <1000 concurrent players
- Simple deployment, no external dependencies

**Future (Multi-Server with Redis Backplane)**:
```csharp
services.AddSignalR()
    .AddStackExchangeRedis(Configuration.GetConnectionString("Redis"));
```

- Supports unlimited scale-out
- Required when exceeding single-server limits
- Adds operational complexity and cost

**Decision**: Start with single-server deployment; add Redis backplane only when load testing indicates need.

### Message Batching

SignalR automatically batches small messages sent within 100ms for efficiency. For high-frequency updates (future feature: live cursor positions), consider manual batching to reduce network overhead.

## Testing Strategy

**Unit Tests**: Mock `IHubCallerClients` and `IGroupManager` for testing hub methods  
**Integration Tests**: Use `HubConnection` in-memory for end-to-end hub testing  
**E2E Tests**: Playwright tests with multiple browser contexts simulating two players

## Performance Metrics

**Target Metrics**:
- **Move Propagation Latency**: <100ms from client A → server → client B
- **Connection Establishment**: <500ms on page load
- **Reconnection Time**: <2 seconds on network interruption
- **Memory per Connection**: ~4-8KB per client

## References

- [SignalR Documentation](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [SignalR Performance Tuning](https://learn.microsoft.com/aspnet/core/signalr/performance)
- [SignalR Redis Backplane](https://learn.microsoft.com/aspnet/core/signalr/scale)

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If concurrent players exceed 500 or latency > 200ms

## Related ADRs
- [ADR-001: Use Blazor WebAssembly for Client Application](./001-blazor-webassembly.md)
- [ADR-005: Adopt Vertical Slice Architecture](./005-vertical-slice-architecture.md)
