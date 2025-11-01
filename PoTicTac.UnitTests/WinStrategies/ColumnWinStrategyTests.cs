using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.UnitTests.WinStrategies;

public class ColumnWinStrategyTests
{
    private readonly ColumnWinStrategy _strategy;

    public ColumnWinStrategyTests()
    {
        _strategy = new ColumnWinStrategy();
    }

    [Fact]
    public void CheckDirection_WithFourInAColumnVertical_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create X wins in first column vertically
        for (int row = 0; row < 4; row++)
        {
            board.Board[row, 0] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckDirection_WithFourInAColumnMiddle_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Create O wins in column 5, starting at row 2
        for (int row = 2; row < 6; row++)
        {
            board.Board[row, 5] = PlayerType.O;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.O);
    }

    [Fact]
    public void CheckDirection_WithThreeInAColumn_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // Only 3 in a column
        for (int row = 0; row < 3; row++)
        {
            board.Board[row, 0] = PlayerType.X;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckDirection_WithFourButBrokenVertically_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        // X-X-O-X pattern vertically is not a win
        board.Board[0, 0] = PlayerType.X;
        board.Board[1, 0] = PlayerType.X;
        board.Board[2, 0] = PlayerType.O;
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
    public void CheckDirection_WithFourInAColumnAtBottomOfBoard_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        int boardSize = GameBoardState.BoardSize;
        // Create O wins in last column, last 4 positions vertically
        for (int row = boardSize - 4; row < boardSize; row++)
        {
            board.Board[row, boardSize - 1] = PlayerType.O;
        }

        // Act
        var result = _strategy.CheckDirection(board, 4);

        // Assert
        result.Should().Be(PlayerType.O);
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
