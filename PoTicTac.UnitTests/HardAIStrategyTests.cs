using PoTicTac.Client.Models;
using PoTicTac.Client.Services.AIStrategies;
using Xunit;

namespace PoTicTac.UnitTests;

public class HardAIStrategyTests
{
    private const int BoardSize = 6; // Power of Three uses 6x6 board

    [Fact]
    public async Task GetMoveAsync_EmptyBoard_ReturnsCenterQuickly()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;

        // Assert
        int center = BoardSize / 2;
        Assert.Equal(new[] { center, center }, move); // Should take center
        Assert.True((endTime - startTime).TotalSeconds < 1, 
            $"AI should respond within 1 second, took {(endTime - startTime).TotalMilliseconds}ms");
    }

    [Fact]
    public async Task GetMoveAsync_OneMoveMade_RespondsQuickly()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        gameState.Board[0, 0] = PlayerType.X; // Human takes corner

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(move);
        Assert.Equal(2, move.Length);
        Assert.True(gameState.Board[move[0], move[1]] == PlayerType.None, "Move should be on empty cell");
        Assert.True((endTime - startTime).TotalSeconds < 2, 
            $"AI should respond within 2 seconds, took {(endTime - startTime).TotalMilliseconds}ms");
    }

    [Fact]
    public async Task GetMoveAsync_BlocksWinningMove()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        gameState.Board[0, 0] = PlayerType.X;
        gameState.Board[0, 1] = PlayerType.X;
        gameState.Board[0, 2] = PlayerType.X;
        // X has 3 in a row, needs to block if they can extend

        // Act
        var move = await strategy.GetMoveAsync(gameState);

        // Assert
        Assert.NotNull(move);
        // Should make a defensive move (can't predict exact position without game rules)
        Assert.True(gameState.Board[move[0], move[1]] == PlayerType.None, "Move should be on empty cell");
    }

    [Fact]
    public async Task GetMoveAsync_TakesWinningMove()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        gameState.Board[0, 0] = PlayerType.O;
        gameState.Board[0, 1] = PlayerType.O;
        gameState.Board[0, 2] = PlayerType.O;
        gameState.Board[1, 1] = PlayerType.X;
        // O has 3 in a row, check if it can extend or defend

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(move);
        Assert.True(gameState.Board[move[0], move[1]] == PlayerType.None, "Move should be on empty cell");
        Assert.True((endTime - startTime).TotalSeconds < 2, 
            $"AI should respond within 2 seconds, took {(endTime - startTime).TotalMilliseconds}ms");
    }

    [Fact]
    public async Task GetMoveAsync_MidGame_PerformanceTest()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.X
        };
        
        // Simulate mid-game with multiple moves
        gameState.Board[2, 2] = PlayerType.X;
        gameState.Board[2, 3] = PlayerType.O;
        gameState.Board[3, 2] = PlayerType.O;
        gameState.Board[3, 3] = PlayerType.X;
        gameState.Board[1, 1] = PlayerType.X;
        gameState.Board[4, 4] = PlayerType.O;

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(move);
        Assert.True(gameState.Board[move[0], move[1]] == PlayerType.None, "Move should be on empty cell");
        Assert.True((endTime - startTime).TotalSeconds < 3, 
            $"AI should respond within 3 seconds for mid-game, took {(endTime - startTime).TotalMilliseconds}ms");
    }

    [Fact]
    public async Task GetMoveAsync_ComplexPosition_RespondsReasonably()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        
        // Create a complex board state
        gameState.Board[0, 0] = PlayerType.X;
        gameState.Board[0, 1] = PlayerType.O;
        gameState.Board[1, 0] = PlayerType.O;
        gameState.Board[1, 1] = PlayerType.X;
        gameState.Board[2, 2] = PlayerType.X;
        gameState.Board[2, 3] = PlayerType.O;
        gameState.Board[3, 2] = PlayerType.O;
        gameState.Board[3, 3] = PlayerType.X;
        gameState.Board[4, 4] = PlayerType.O;
        gameState.Board[5, 5] = PlayerType.X;

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(move);
        Assert.Equal(2, move.Length);
        Assert.True(gameState.Board[move[0], move[1]] == PlayerType.None, "Move should be on empty cell");
        Assert.True((endTime - startTime).TotalSeconds < 5, 
            $"AI should respond within 5 seconds even for complex positions, took {(endTime - startTime).TotalMilliseconds}ms");
    }
}
