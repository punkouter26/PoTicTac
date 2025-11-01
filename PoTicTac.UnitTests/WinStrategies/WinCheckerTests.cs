using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.UnitTests.WinStrategies;

public class WinCheckerTests
{
    private readonly WinChecker _winChecker;

    public WinCheckerTests()
    {
        _winChecker = new WinChecker();
    }

    [Fact]
    public void CheckWinner_WithHorizontalWin_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        for (int col = 0; col < 4; col++)
        {
            board.Board[0, col] = PlayerType.X;
        }

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckWinner_WithVerticalWin_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        for (int row = 0; row < 4; row++)
        {
            board.Board[row, 0] = PlayerType.O;
        }

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.O);
    }

    [Fact]
    public void CheckWinner_WithDiagonalDownWin_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        for (int i = 0; i < 4; i++)
        {
            board.Board[i, i] = PlayerType.X;
        }

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    public void CheckWinner_WithDiagonalUpWin_ReturnsWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        for (int i = 0; i < 4; i++)
        {
            board.Board[i, 3 - i] = PlayerType.O;
        }

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.O);
    }

    [Fact]
    public void CheckWinner_WithEmptyBoard_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckWinner_WithPartialBoard_ReturnsNull()
    {
        // Arrange
        var board = CreateEmptyBoard();
        board.Board[0, 0] = PlayerType.X;
        board.Board[0, 1] = PlayerType.O;
        board.Board[1, 0] = PlayerType.O;
        board.Board[1, 1] = PlayerType.X;

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckWinner_WithFullBoardNoWinner_ReturnsDraw()
    {
        // Arrange
        var board = CreateEmptyBoard();
        
        // Create a specific full board pattern with no 4-in-a-row
        // Each row, column, and diagonal carefully verified to avoid wins
        // Row 0: X O X O X O
        // Row 1: O X O X O X  
        // Row 2: O X O X O X
        // Row 3: X O X O X O
        // Row 4: X O X O X O
        // Row 5: O X O X O X
        
        board.Board[0, 0] = PlayerType.X;
        board.Board[0, 1] = PlayerType.O;
        board.Board[0, 2] = PlayerType.X;
        board.Board[0, 3] = PlayerType.O;
        board.Board[0, 4] = PlayerType.X;
        board.Board[0, 5] = PlayerType.O;
        
        board.Board[1, 0] = PlayerType.O;
        board.Board[1, 1] = PlayerType.X;
        board.Board[1, 2] = PlayerType.O;
        board.Board[1, 3] = PlayerType.X;
        board.Board[1, 4] = PlayerType.O;
        board.Board[1, 5] = PlayerType.X;
        
        board.Board[2, 0] = PlayerType.O;
        board.Board[2, 1] = PlayerType.X;
        board.Board[2, 2] = PlayerType.O;
        board.Board[2, 3] = PlayerType.X;
        board.Board[2, 4] = PlayerType.O;
        board.Board[2, 5] = PlayerType.X;
        
        board.Board[3, 0] = PlayerType.X;
        board.Board[3, 1] = PlayerType.O;
        board.Board[3, 2] = PlayerType.X;
        board.Board[3, 3] = PlayerType.O;
        board.Board[3, 4] = PlayerType.X;
        board.Board[3, 5] = PlayerType.O;
        
        board.Board[4, 0] = PlayerType.X;
        board.Board[4, 1] = PlayerType.O;
        board.Board[4, 2] = PlayerType.X;
        board.Board[4, 3] = PlayerType.O;
        board.Board[4, 4] = PlayerType.X;
        board.Board[4, 5] = PlayerType.O;
        
        board.Board[5, 0] = PlayerType.O;
        board.Board[5, 1] = PlayerType.X;
        board.Board[5, 2] = PlayerType.O;
        board.Board[5, 3] = PlayerType.X;
        board.Board[5, 4] = PlayerType.O;
        board.Board[5, 5] = PlayerType.X;

        // Act
        var result = _winChecker.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.Draw);
    }

    [Fact]
    public void CheckWinner_WithMultiplePotentialWinsButOnlyOneComplete_ReturnsFirstWinner()
    {
        // Arrange
        var board = CreateEmptyBoard();
        
        // Complete horizontal win for X
        for (int col = 0; col < 4; col++)
        {
            board.Board[0, col] = PlayerType.X;
        }
        
        // Incomplete vertical for O (only 3)
        for (int row = 0; row < 3; row++)
        {
            board.Board[row, 5] = PlayerType.O;
        }

        // Act
        var result = _winChecker.CheckWinner(board);

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
