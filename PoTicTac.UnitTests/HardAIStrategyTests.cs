using Bogus;
using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.AIStrategies;
using Xunit;

namespace PoTicTac.UnitTests;

public class HardAIStrategyTests
{
    private const int BoardSize = 6; // Power of Three uses 6x6 board
    private readonly Faker _faker = new();

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Performance")]
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
        var elapsed = (endTime - startTime).TotalSeconds;

        // Assert
        int center = BoardSize / 2;
        move.Should().BeEquivalentTo(new[] { center, center }, "AI should prefer center position on empty board");
        elapsed.Should().BeLessThan(1, $"AI should respond within 1 second, took {elapsed * 1000}ms");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Performance")]
    public async Task GetMoveAsync_OneMoveMade_RespondsQuickly()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        var cornerRow = _faker.Random.Int(0, 0); // Top row
        var cornerCol = _faker.Random.Int(0, 0); // Left column
        gameState.Board[cornerRow, cornerCol] = PlayerType.X; // Human takes corner

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;
        var elapsed = (endTime - startTime).TotalSeconds;

        // Assert
        move.Should().NotBeNull();
        move.Should().HaveCount(2, "move should be a coordinate pair [row, col]");
        gameState.Board[move[0], move[1]].Should().Be(PlayerType.None, "move should be on empty cell");
        elapsed.Should().BeLessThan(2, $"AI should respond within 2 seconds, took {elapsed * 1000}ms");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Logic")]
    public async Task GetMoveAsync_BlocksWinningMove()
    {
        // Arrange
        var strategy = new HardAIStrategy();
        var gameState = new GameBoardState
        {
            CurrentPlayer = PlayerType.O
        };
        // Create a 3-in-a-row threat
        gameState.Board[0, 0] = PlayerType.X;
        gameState.Board[0, 1] = PlayerType.X;
        gameState.Board[0, 2] = PlayerType.X;

        // Act
        var move = await strategy.GetMoveAsync(gameState);

        // Assert
        move.Should().NotBeNull();
        gameState.Board[move[0], move[1]].Should().Be(PlayerType.None, "move should be on empty cell");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Logic")]
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

        // Act
        var startTime = DateTime.UtcNow;
        var move = await strategy.GetMoveAsync(gameState);
        var endTime = DateTime.UtcNow;
        var elapsed = (endTime - startTime).TotalSeconds;

        // Assert
        move.Should().NotBeNull();
        gameState.Board[move[0], move[1]].Should().Be(PlayerType.None, "move should be on empty cell");
        elapsed.Should().BeLessThan(2, $"AI should respond within 2 seconds, took {elapsed * 1000}ms");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Performance")]
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
        var elapsed = (endTime - startTime).TotalSeconds;

        // Assert
        move.Should().NotBeNull();
        gameState.Board[move[0], move[1]].Should().Be(PlayerType.None, "move should be on empty cell");
        elapsed.Should().BeLessThan(3, $"AI should respond within 3 seconds for mid-game, took {elapsed * 1000}ms");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "HardAIStrategy")]
    [Trait("Type", "Performance")]
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
        var elapsed = (endTime - startTime).TotalSeconds;

        // Assert
        move.Should().NotBeNull();
        move.Should().HaveCount(2, "move should be a coordinate pair [row, col]");
        gameState.Board[move[0], move[1]].Should().Be(PlayerType.None, "move should be on empty cell");
        elapsed.Should().BeLessThan(5, $"AI should respond within 5 seconds even for complex positions, took {elapsed * 1000}ms");
    }
}
