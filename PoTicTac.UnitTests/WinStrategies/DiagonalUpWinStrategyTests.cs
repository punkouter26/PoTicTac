using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.UnitTests.WinStrategies;

public class DiagonalUpWinStrategyTests
{
    private readonly DiagonalUpWinStrategy _strategy;

    public DiagonalUpWinStrategyTests()
    {
        _strategy = new DiagonalUpWinStrategy();
    }

    [Fact]
    public void CheckDirection_WithFourInADiagonalTopRightToBottomLeft_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create X wins diagonally from (0,3) down-left to (3,0)
        for (int i = 0; i < 4; i++)
        {
            board.Board[i, 3 - i] = PlayerType.X;
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
        // Create O wins diagonally from (1,4) down-left to (4,1)
        for (int i = 0; i < 4; i++)
        {
            board.Board[1 + i, 4 - i] = PlayerType.O;
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
            board.Board[i, 3 - i] = PlayerType.X;
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
        // X-X-O-X diagonally (top-right to bottom-left) is not a win
        board.Board[0, 3] = PlayerType.X;
        board.Board[1, 2] = PlayerType.X;
        board.Board[2, 1] = PlayerType.O;
        board.Board[3, 0] = PlayerType.X;

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
    public void CheckDirection_WithFourInADiagonalBottomArea_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        int boardSize = GameBoardState.BoardSize;
        // Create X wins diagonally from (boardSize-4, boardSize-1) down-left
        int startRow = boardSize - 4;
        int startCol = boardSize - 1;
        for (int i = 0; i < 4; i++)
        {
            board.Board[startRow + i, startCol - i] = PlayerType.X;
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
        board.Board[0, 4] = PlayerType.O;
        board.Board[1, 3] = PlayerType.O;
        board.Board[2, 2] = PlayerType.O;
        
        // Complete diagonal from (1,5) down-left to (4,2)
        for (int i = 0; i < 4; i++)
        {
            board.Board[1 + i, 5 - i] = PlayerType.X;
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
