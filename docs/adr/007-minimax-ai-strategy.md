# ADR-007: Implement Minimax Algorithm for Hard AI

## Status
**Accepted** - August 2, 2025

## Context

We needed to implement three difficulty levels of AI opponents for single-player mode in PoTicTac. The requirements were:

- **Easy Mode**: Beginner-friendly AI that makes mistakes but isn't completely random
- **Medium Mode**: Intermediate AI that provides a fair challenge
- **Hard Mode**: Expert-level AI that plays optimally or near-optimally
- **Extensibility**: Easy to add new AI strategies in the future
- **Performance**: AI move calculation must complete in <2 seconds (preferably <500ms)
- **Code Clarity**: Implementation should be understandable for educational purposes

The game board is **6x6 with 4-in-a-row** to win, which creates a much larger search space than traditional 3x3 Tic Tac Toe:
- **Board States**: 36 positions = ~10^28 possible states (vs. 3x3's ~10^5)
- **Branching Factor**: Up to 36 possible moves early game
- **Average Game Length**: ~15-20 moves (vs. 3x3's ~9 moves)

## Decision

We will implement the **Minimax algorithm with alpha-beta pruning** for Hard AI difficulty.

### AI Strategy Pattern

```csharp
public interface IAIStrategy
{
    Task<(int row, int col)> GetBestMoveAsync(string[,] board, string aiSymbol);
}
```

### Three Implementations

**1. EasyAIStrategy** (Random with Basic Blocking)
```csharp
- 70% random moves from available positions
- 30% strategic moves (block immediate win threats)
- No lookahead
- Execution time: <10ms
```

**2. MediumAIStrategy** (Threat Detection)
```csharp
- Detect immediate win opportunities (1-ply lookahead)
- Block opponent's immediate wins
- Prefer center and corner positions
- Basic pattern recognition (3-in-a-row threats)
- Execution time: <50ms
```

**3. HardAIStrategy** (Minimax with Alpha-Beta Pruning)
```csharp
public async Task<(int row, int col)> GetBestMoveAsync(string[,] board, string aiSymbol)
{
    const int MAX_DEPTH = 4;  // 4-ply lookahead
    
    int bestScore = int.MinValue;
    (int row, int col) bestMove = (-1, -1);
    
    foreach (var move in GetAvailableMoves(board))
    {
        MakeMove(board, move, aiSymbol);
        int score = Minimax(board, MAX_DEPTH - 1, int.MinValue, int.MaxValue, false, aiSymbol, opponentSymbol);
        UndoMove(board, move);
        
        if (score > bestScore)
        {
            bestScore = score;
            bestMove = move;
        }
    }
    
    return bestMove;
}

private int Minimax(string[,] board, int depth, int alpha, int beta, bool isMaximizing, string aiSymbol, string opponentSymbol)
{
    // Terminal conditions
    if (depth == 0 || GameOver(board))
        return EvaluateBoard(board, aiSymbol, opponentSymbol);
    
    if (isMaximizing)
    {
        int maxScore = int.MinValue;
        foreach (var move in GetAvailableMoves(board))
        {
            MakeMove(board, move, aiSymbol);
            int score = Minimax(board, depth - 1, alpha, beta, false, aiSymbol, opponentSymbol);
            UndoMove(board, move);
            
            maxScore = Math.Max(maxScore, score);
            alpha = Math.Max(alpha, score);
            if (beta <= alpha) break;  // Alpha-beta pruning
        }
        return maxScore;
    }
    else
    {
        int minScore = int.MaxValue;
        foreach (var move in GetAvailableMoves(board))
        {
            MakeMove(board, move, opponentSymbol);
            int score = Minimax(board, depth - 1, alpha, beta, true, aiSymbol, opponentSymbol);
            UndoMove(board, move);
            
            minScore = Math.Min(minScore, score);
            beta = Math.Min(beta, score);
            if (beta <= alpha) break;  // Alpha-beta pruning
        }
        return minScore;
    }
}
```

### Evaluation Function

```csharp
private int EvaluateBoard(string[,] board, string aiSymbol, string opponentSymbol)
{
    if (CheckWin(board, aiSymbol)) return +1000;
    if (CheckWin(board, opponentSymbol)) return -1000;
    
    int score = 0;
    
    // Evaluate all possible 4-in-a-row patterns
    score += EvaluateLines(board, aiSymbol) * 10;
    score -= EvaluateLines(board, opponentSymbol) * 10;
    
    // Prefer center positions
    score += (board[3, 3] == aiSymbol) ? 5 : 0;
    score -= (board[3, 3] == opponentSymbol) ? 5 : 0;
    
    return score;
}
```

## Consequences

### Positive

✅ **Optimal Play (with depth limit)**: Minimax guarantees best move within search depth  
✅ **Educational Value**: Classic AI algorithm, well-documented, easy to understand  
✅ **Extensibility**: Strategy pattern allows easy addition of new AI levels  
✅ **Testability**: Deterministic algorithm produces predictable results  
✅ **Performance**: Alpha-beta pruning reduces search space by ~50-70%  
✅ **Challenge**: Hard AI is genuinely challenging for most players  

### Negative

⚠️ **Computational Complexity**: O(b^d) where b = branching factor (~20), d = depth (4)  
⚠️ **Execution Time**: 500ms-2 seconds for depth 4 on 6x6 board  
⚠️ **Memory Usage**: Recursive call stack grows with depth  
⚠️ **Not Truly Optimal**: Depth limit means AI doesn't search entire game tree  
⚠️ **Opening Book Needed**: No pre-computed openings; calculates from scratch each time  

### Trade-offs

- **Optimality vs. Speed**: Depth limit (4-ply) for acceptable performance
- **Complexity vs. Strength**: More complex than heuristics but stronger gameplay
- **Determinism vs. Variety**: Same position always produces same move (no randomness)

## Alternatives Considered

### 1. Random Moves Only
**Pros**: Instant calculation, trivial implementation, always loses  
**Cons**: **Too easy**, no challenge, boring for players  
**Why Rejected**: Doesn't provide Hard difficulty option

### 2. Rule-Based Heuristics (Only)
**Pros**: Fast, predictable, easier to understand  
**Cons**: **Not optimal**, easily exploitable patterns, caps at intermediate strength  
**Why Rejected**: Can't reach expert-level play; players would discover exploits

### 3. Monte Carlo Tree Search (MCTS)
**Pros**: State-of-the-art for games, highly scalable, handles large branching factors well  
**Cons**: **More complex** to implement, requires many simulations, harder to tune  
**Why Rejected**: Overkill for Tic Tac Toe variant; Minimax is simpler and sufficient

### 4. Neural Network (Deep Learning)
**Pros**: Can learn optimal strategies, trendy, impressive  
**Cons**: **Massive over-engineering**, requires training data, training time, model hosting, unpredictable behavior  
**Why Rejected**: Completely overkill for a solved game; Minimax is provably optimal

### 5. Negamax (Simplified Minimax)
**Pros**: Cleaner code (single recursive function), mathematically equivalent to Minimax  
**Cons**: Slightly harder to understand for beginners  
**Why Rejected**: Too similar to Minimax; chose Minimax for better educational clarity

### 6. Iterative Deepening
**Pros**: Guarantees move within time limit, better move ordering  
**Cons**: Re-explores shallow depths repeatedly  
**Why Rejected**: Fixed depth is sufficient for our use case; added complexity not justified

## Implementation Notes

### Performance Optimizations

**1. Alpha-Beta Pruning**
- Reduces search space by 50-70%
- Critical for 6x6 board performance

**2. Move Ordering**
- Evaluate center moves first (more likely to be optimal)
- Increases alpha-beta cutoffs

**3. Depth Limiting**
- Hard cap at depth 4 (searches 4 moves ahead)
- Prevents excessive computation on early game

**4. Asynchronous Execution**
```csharp
public async Task<(int row, int col)> GetBestMoveAsync(...)
{
    return await Task.Run(() => GetBestMove(...));
}
```
- Runs on background thread (doesn't block UI)
- Allows progress indicators on client

### Depth Selection Rationale

| Depth | Positions Evaluated | Execution Time | Play Quality |
|-------|---------------------|----------------|--------------|
| 2 | ~400 | 10ms | Weak, easily beaten |
| 3 | ~8,000 | 150ms | Intermediate |
| **4** | **~160,000** | **500ms-2s** | **Strong (chosen)** |
| 5 | ~3,200,000 | 10-30s | Very strong (too slow) |
| 6 | ~64,000,000 | 3-10 minutes | Optimal (impractical) |

**Decision**: Depth 4 balances strength and performance.

### Testing Strategy

**Unit Tests**:
```csharp
[Fact]
public async Task HardAI_BlocksImmediateWin()
{
    // Arrange: Board with opponent one move from winning
    var board = new string[6, 6];
    board[0, 0] = "X"; board[0, 1] = "X"; board[0, 2] = "X";
    // board[0, 3] is empty (opponent's winning move)
    
    // Act
    var move = await hardAI.GetBestMoveAsync(board, "O");
    
    // Assert: AI must block at (0, 3)
    Assert.Equal((0, 3), move);
}
```

**Performance Tests**:
```csharp
[Fact]
public async Task HardAI_CompletesWithin2Seconds()
{
    var stopwatch = Stopwatch.StartNew();
    var move = await hardAI.GetBestMoveAsync(emptyBoard, "O");
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 2000);
}
```

### Evaluation Function Tuning

**Weight Values** (empirically tuned):
```csharp
Win:           +1000
Loss:          -1000
3-in-a-row:    +10 per occurrence
2-in-a-row:    +5 per occurrence
Center square: +5
```

These values were tuned through playtesting to balance offensive and defensive play.

## Future Enhancements

### 1. Opening Book
Pre-compute optimal first 3-4 moves:
```csharp
private static Dictionary<string, (int, int)> OpeningBook = new()
{
    ["empty_board"] = (3, 3),  // Always start in center
    ["opponent_center"] = (2, 2),  // Corner response
};
```

**Benefit**: Instant move for first 2-3 turns (no computation)

### 2. Transposition Table (Memoization)
Cache previously evaluated board states:
```csharp
private Dictionary<string, int> _transpositionTable = new();

private int Minimax(...)
{
    string boardHash = GetBoardHash(board);
    if (_transpositionTable.TryGetValue(boardHash, out int cachedScore))
        return cachedScore;
    
    // ... compute score ...
    
    _transpositionTable[boardHash] = score;
    return score;
}
```

**Benefit**: Reduces repeated computation by 30-50%

### 3. Progressive Deepening
Gradually increase depth until time limit:
```csharp
public async Task<(int row, int col)> GetBestMoveAsync(TimeSpan timeLimit)
{
    for (int depth = 1; depth <= MAX_DEPTH; depth++)
    {
        if (elapsed > timeLimit) break;
        bestMove = SearchAtDepth(depth);
    }
    return bestMove;
}
```

**Benefit**: Guarantees move within time budget

## References

- [Minimax Algorithm](https://en.wikipedia.org/wiki/Minimax)
- [Alpha-Beta Pruning](https://en.wikipedia.org/wiki/Alpha–beta_pruning)
- [Negamax](https://en.wikipedia.org/wiki/Negamax) (simplified variant)
- [Tic-Tac-Toe AI](https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/)

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If player complaints about AI difficulty or execution time > 3 seconds

## Related ADRs
- [ADR-001: Use Blazor WebAssembly for Client Application](./001-blazor-webassembly.md)
- [ADR-005: Adopt Vertical Slice Architecture](./005-vertical-slice-architecture.md)
- [ADR-008: 6x6 Board with 4-in-a-Row Victory](./008-6x6-board-4-in-a-row.md)
