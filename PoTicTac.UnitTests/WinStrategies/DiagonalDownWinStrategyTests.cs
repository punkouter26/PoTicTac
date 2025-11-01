using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.UnitTests.WinStrategies;

public class DiagonalDownWinStrategyTests
{
    private readonly DiagonalDownWinStrategy _strategy;

    public DiagonalDownWinStrategyTests()
    {
        _strategy = new DiagonalDownWinStrategy();
    }

    [Fact]
    public void CheckDirection_WithFourInADiagonalTopLeftToBottomRight_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create X wins diagonally from (0,0) to (3,3)
        for (int i = 0; i < 4; i++)
        {
            board.Board[i, i] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckDirection_WithFourInADiagonalMiddleOfBoard_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create O wins diagonally from (1,1) to (4,4)
        for (int i = 0; i < 4; i++)
        {
            board.Board[1 + i, 1 + i] = PlayerType.O;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.O);
    }

    [Fact]
    public void CheckDirection_WithThreeInADiagonal_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Only 3 in a diagonal
        for (int i = 0; i < 3; i++)
        {
            board.Board[i, i] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckDirection_WithFourButBrokenDiagonal_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // X-X-O-X diagonally is not a win
        board.Board[0, 0] = PlayerType.X;
        board.Board[1, 1] = PlayerType.X;
        board.Board[2, 2] = PlayerType.O;
        board.Board[3, 3] = PlayerType.X;

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckDirection_WithEmptyBoard_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckDirection_WithFourInADiagonalBottomRightCorner_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        int boardSize = GameBoardState.BoardSize;
        // Create X wins diagonally in bottom-right area
        int startPos = boardSize - 4;
        for (int i = 0; i < 4; i++)
        {
            board.Board[startPos + i, startPos + i] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckDirection_WithMultipleDiagonalsOnlyOneComplete_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Incomplete diagonal
        board.Board[0, 0] = PlayerType.O;
        board.Board[1, 1] = PlayerType.O;
        board.Board[2, 2] = PlayerType.O;
        
        // Complete diagonal (1,1) to (4,4)
        for (int i = 1; i < 5; i++)
        {
            board.Board[i, i] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    private static GameBoardState CreateEmptyBoard()
    {
        return new GameBoardState
        {
            CurrentPlayer = PlayerType.X,
            GameStatus = GameStatus.Playing,
            Winner = null,
            Players = new[]
            {
                new Player { Type = PlayerType.X, Name = "Player X" },
                new Player { Type = PlayerType.O, Name = "Player O" }
            },
            MoveHistory = new List<Move>(),
            UndoStack = new List<Move>(),
            RedoStack = new List<Move>(),
            SessionStats = new SessionStats
            {
                TotalGames = 0,
                StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };
    }
}
