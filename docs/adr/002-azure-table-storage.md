# ADR-002: Use Azure Table Storage for Data Persistence

## Status
**Accepted** - August 2, 2025

## Context

We needed a data persistence solution for storing player statistics in PoTicTac. The requirements were:

- **Simple Data Model**: Key-value storage for player statistics (name → stats)
- **Low Cost**: Minimal operational costs for hobby/demonstration project
- **Fast Lookups**: Quick retrieval of individual player statistics
- **Scalability**: Ability to handle growing player base without infrastructure changes
- **Azure Integration**: Seamless integration with Azure ecosystem
- **Local Development**: Easy local emulation for development without cloud costs
- **Simple Queries**: Basic filtering and sorting for leaderboards
- **No Complex Relationships**: No need for joins, foreign keys, or complex transactions

The data model is straightforward:
- **Entity**: PlayerStats (PartitionKey: "Players", RowKey: PlayerName)
- **Operations**: Get by name, Save/Update, GetAll, GetLeaderboard (sorted by WinRate)
- **Data Volume**: Estimated thousands of players, not millions

## Decision

We will use **Azure Table Storage** as the primary data persistence layer for player statistics.

### Storage Configuration

**Table Structure**:
```
Table: PoTicTacPlayerStats
├── PartitionKey: "Players" (all players in single partition)
├── RowKey: PlayerName (unique identifier)
└── Properties:
    ├── Wins: int
    ├── Losses: int
    ├── Draws: int
    ├── TotalGames: int
    ├── WinRate: double
    ├── WinStreak: int
    ├── CurrentStreak: int
    └── AverageMovesPerGame: double
```

**Local Development**: Azurite emulator with connection string `UseDevelopmentStorage=true`  
**Production**: Azure Table Storage with connection string from App Service configuration

## Consequences

### Positive

✅ **Cost-Effective**: ~$0.05-0.20/month for typical usage (far cheaper than SQL Database)  
✅ **Simple Schema**: No database migrations or schema management overhead  
✅ **Fast Single-Entity Lookups**: O(1) lookups by PartitionKey + RowKey  
✅ **Automatic Scaling**: Azure handles scaling transparently  
✅ **High Availability**: Built-in geo-redundancy and 99.9% SLA  
✅ **Easy Local Development**: Azurite provides identical API for local testing  
✅ **Minimal Code**: Simple `TableClient` API with LINQ-like queries  
✅ **No Connection Pooling**: Stateless HTTP-based protocol eliminates connection management  
✅ **JSON Serialization**: Automatic serialization of C# objects to table entities  

### Negative

⚠️ **Limited Query Capabilities**: No joins, aggregations, or complex filtering (must filter in-memory)  
⚠️ **Single Partition Limitations**: All players in one partition limits throughput to ~2000 ops/sec (acceptable for this scale)  
⚠️ **No Transactions Across Partitions**: Entity Group Transactions limited to same partition (not a concern with single partition)  
⚠️ **Basic Indexing**: Only PartitionKey and RowKey are indexed; other filters require table scans  
⚠️ **Eventual Consistency**: Replication can introduce slight delays (rarely noticeable)  
⚠️ **Property Limitations**: Max 252 properties per entity, 1MB entity size (far more than needed)  

### Trade-offs

- **Query Flexibility vs. Simplicity**: Limited queries for easier management and lower cost
- **Throughput vs. Cost**: Single partition limits throughput but is sufficient for expected load
- **Relational Features vs. Performance**: No SQL overhead for simple key-value lookups

## Alternatives Considered

### 1. Azure SQL Database
**Pros**: Full relational capabilities, complex queries, transactions, T-SQL, familiar to most developers  
**Cons**: **High cost** (~$5/month minimum for Basic tier), overkill for simple key-value data, connection pooling complexity, schema migrations  
**Why Rejected**: 50-100x more expensive than Table Storage for a feature set we don't need

### 2. Azure Cosmos DB
**Pros**: Global distribution, multiple consistency models, rich queries, multi-model support (SQL, MongoDB, Cassandra)  
**Cons**: **Higher cost** (~$25/month minimum with serverless), overkill for simple use case, more complex API  
**Why Rejected**: Too expensive and feature-rich for our simple data model; Table Storage is a subset of Cosmos DB capabilities at much lower cost

### 3. Entity Framework Core with SQLite
**Pros**: Relational model, familiar ORM, offline-first, file-based, zero cost  
**Cons**: File locking issues in multi-instance scenarios, no cloud-native scaling, manual backup, poor for Azure App Service  
**Why Rejected**: Not cloud-native; doesn't scale horizontally; file locking issues with multiple app instances

### 4. MongoDB Atlas (Free Tier)
**Pros**: Document model, free tier (512MB), JSON-native, flexible schema  
**Cons**: Requires separate account management, 512MB limit quickly exceeded, connection string management, not Azure-native  
**Why Rejected**: Adds external dependency outside Azure ecosystem; free tier limitations

### 5. In-Memory Dictionary with File Persistence
**Pros**: Fastest possible lookups, zero cloud costs, simple implementation  
**Cons**: Data loss on app restart (unless persisted to file), no multi-instance support, no geo-redundancy  
**Why Rejected**: Not production-ready; data loss risk unacceptable

### 6. Redis Cache
**Pros**: Extremely fast, in-memory performance, pub/sub capabilities, data structures  
**Cons**: Higher cost (~$15/month for Basic), not designed for primary storage (volatile), requires backup strategy  
**Why Rejected**: Designed for caching, not primary persistence; overkill for our throughput needs

## Implementation Notes

### StorageService Abstraction
```csharp
public class StorageService
{
    private readonly TableClient _tableClient;

    public async Task<PlayerStats?> GetPlayerStatsAsync(string playerName);
    public async Task SavePlayerStatsAsync(string playerName, PlayerStats stats);
    public async Task<List<PlayerEntity>> GetAllPlayersAsync();
    public async Task<List<PlayerEntity>> GetLeaderboardAsync(int limit);
}
```

### Connection String Management
- **Local**: `UseDevelopmentStorage=true` (Azurite emulator)
- **Production**: Injected via App Service Application Settings → Key Vault reference (future enhancement)

### Entity Mapping
```csharp
public class PlayerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Players";  // Single partition
    public string RowKey { get; set; } = string.Empty;     // PlayerName
    public string Name => RowKey;
    public PlayerStats Stats { get; set; } = new();
    // ITableEntity properties: Timestamp, ETag
}
```

### Leaderboard Query Performance
Current implementation:
1. Fetch all players from table (typically <1000 players)
2. Sort in-memory by WinRate descending
3. Take top N players

**Why Acceptable**: 
- Small dataset (<1000 players) makes in-memory sorting negligible
- Leaderboard requests are infrequent (compared to game moves)
- Azure Table Storage's fast enumeration (~10-20ms for 1000 entities)

**Future Optimization** (if needed):
- Add secondary index with reverse WinRate in RowKey for sorted retrieval
- Implement caching layer (MemoryCache) for leaderboard results

### Cost Analysis (Monthly Estimates)

**Scenario**: 1000 players, 100 new games/day

| Operation | Count/Month | Cost |
|-----------|-------------|------|
| Storage (1000 players × 1KB) | 1 MB | $0.0001 |
| Writes (100 games/day × 2 players) | 6,000 | $0.003 |
| Reads (leaderboard 100×/day) | 3,000 | $0.0002 |
| **Total** | | **~$0.05/month** |

For comparison:
- **Azure SQL Basic**: ~$5/month (100x more expensive)
- **Cosmos DB Serverless**: ~$25/month (500x more expensive)

## Migration Path (If Needed)

If we outgrow Azure Table Storage (unlikely), migration path would be:

1. **Export all PlayerEntity data** to JSON
2. **Provision Azure Cosmos DB** (Table API for minimal code changes)
3. **Import data** using Cosmos DB Data Migration Tool
4. **Update connection string** (Table API is compatible with Table Storage client)
5. **Zero code changes** (same `TableClient` API)

## References

- [Azure Table Storage Documentation](https://learn.microsoft.com/azure/storage/tables/table-storage-overview)
- [Azurite Local Emulator](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
- [Azure.Data.Tables SDK](https://www.nuget.org/packages/Azure.Data.Tables)
- [Designing a Scalable Partitioning Strategy](https://learn.microsoft.com/azure/storage/tables/table-storage-design)

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If player count exceeds 10,000 or leaderboard queries become slow (>500ms)

## Related ADRs
- [ADR-006: Use Azure App Service for Hosting](./006-azure-app-service-hosting.md)
