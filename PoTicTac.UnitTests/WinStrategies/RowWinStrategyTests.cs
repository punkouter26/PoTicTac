using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.UnitTests.WinStrategies;

public class RowWinStrategyTests
{
    private readonly RowWinStrategy _strategy;

    public RowWinStrategyTests()
    {
        _strategy = new RowWinStrategy();
    }

    [Fact]
    public void CheckDirection_WithFourInARowHorizontal_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create X wins in top row: XXXX------...
        for (int col = 0; col < 4; col++)
        {
            board.Board[0, col] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckDirection_WithFourInARowMiddleRow_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create O wins in row 3, starting at column 2
        for (int col = 2; col < 6; col++)
        {
            board.Board[3, col] = PlayerType.O;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.O);
    }

    [Fact]
    public void CheckDirection_WithThreeInARow_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Only 3 in a row: XXX-------...
        for (int col = 0; col < 3; col++)
        {
            board.Board[0, col] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckDirection_WithFourButBroken_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // XXOX is not a win
        board.Board[0, 0] = PlayerType.X;
        board.Board[0, 1] = PlayerType.X;
        board.Board[0, 2] = PlayerType.O;
        board.Board[0, 3] = PlayerType.X;

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
    public void CheckDirection_WithFourInARowAtEndOfBoard_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        int boardSize = GameBoardState.BoardSize;
        // Create X wins in last row, last 4 positions
        for (int col = boardSize - 4; col < boardSize; col++)
        {
            board.Board[boardSize - 1, col] = PlayerType.X;
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
