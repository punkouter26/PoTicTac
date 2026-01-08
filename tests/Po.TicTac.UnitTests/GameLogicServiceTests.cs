using Bogus;
using FluentAssertions;
using PoTicTac.Client.Models;
using PoTicTac.Client.Services;
using Xunit;

namespace PoTicTac.UnitTests;

[Trait("Category", "Unit")]
[Trait("Component", "GameLogic")]
public class GameLogicServiceTests
{
    private readonly GameLogicService _service;
    private readonly Faker _faker;

    public GameLogicServiceTests()
    {
        _service = new GameLogicService();
        _faker = new Faker();
    }

    [Fact]
    [Trait("Type", "Logic")]
    public void CheckWinner_EmptyBoard_ReturnsNull()
    {
        // Arrange
        var board = new GameBoardState();

        // Act
        var result = _service.CheckWinner(board);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Type", "Logic")]
    public void CheckWinner_HorizontalWin_ReturnsWinner()
    {
        // Arrange
        var board = new GameBoardState
        {
            Board = new[,]
            {
                { PlayerType.X, PlayerType.X, PlayerType.X, PlayerType.X, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None }
            }
        };

        // Act
        var result = _service.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    [Trait("Type", "Logic")]
    public void CheckWinner_VerticalWin_ReturnsWinner()
    {
        // Arrange
        var board = new GameBoardState
        {
            Board = new[,]
            {
                { PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None }
            }
        };

        // Act
        var result = _service.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    [Trait("Type", "Logic")]
    public void CheckWinner_DiagonalWin_ReturnsWinner()
    {
        // Arrange
        var board = new GameBoardState
        {
            Board = new[,]
            {
                { PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.X, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.X, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None },
                { PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None, PlayerType.None }
            }
        };

        // Act
        var result = _service.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.X);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "GameLogicService")]
    [Trait("Type", "Logic")]
    public void CheckWinner_Draw_ReturnsDraw()
    {
        // Arrange - Board is full but no 4-in-a-row exists
        var board = new GameBoardState
        {
            Board = new[,]
            {
                { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O },
                { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X, PlayerType.X },
                { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O },
                { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X, PlayerType.X },
                { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O },
                { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X, PlayerType.X }
            }
        };

        // Act
        var result = _service.CheckWinner(board);

        // Assert
        result.Should().Be(PlayerType.Draw, "board is full with no 4-in-a-row");
    }

    [Fact]
    [Trait("Type", "StateManagement")]
    public void UndoMove_WithMoveHistory_UndoesLastMove()
    {
        // Arrange
        var players = GenerateRandomPlayers();
        var initialState = _service.CreateInitialState(PlayerType.X, players);
        var afterMove = _service.MakeMove(initialState, 0, 0);

        // Act
        var afterUndo = _service.UndoMove(afterMove);

        // Assert
        afterUndo.Board[0, 0].Should().Be(PlayerType.None);
        afterUndo.MoveHistory.Should().BeEmpty();
        afterUndo.UndoStack.Should().ContainSingle();
        afterUndo.CurrentPlayer.Should().Be(PlayerType.X);
    }

    [Fact]
    [Trait("Type", "StateManagement")]
    public void UndoMove_NoMoves_ReturnsSameState()
    {
        // Arrange
        var players = GenerateRandomPlayers();
        var initialState = _service.CreateInitialState(PlayerType.X, players);

        // Act
        var afterUndo = _service.UndoMove(initialState);

        // Assert
        afterUndo.Should().Be(initialState);
    }

    [Fact]
    [Trait("Type", "StateManagement")]
    public void RedoMove_WithUndoneMove_RedoesMove()
    {
        // Arrange
        var players = GenerateRandomPlayers();
        var initialState = _service.CreateInitialState(PlayerType.X, players);
        var afterMove = _service.MakeMove(initialState, 0, 0);
        var afterUndo = _service.UndoMove(afterMove);

        // Act
        var afterRedo = _service.RedoMove(afterUndo);

        // Assert
        afterRedo.Board[0, 0].Should().Be(PlayerType.X);
        afterRedo.MoveHistory.Should().ContainSingle();
        afterRedo.UndoStack.Should().BeEmpty();
        afterRedo.CurrentPlayer.Should().Be(PlayerType.O);
    }

    [Fact]
    [Trait("Type", "StateManagement")]
    public void RedoMove_NoUndoneMoves_ReturnsSameState()
    {
        // Arrange
        var players = GenerateRandomPlayers();
        var initialState = _service.CreateInitialState(PlayerType.X, players);

        // Act
        var afterRedo = _service.RedoMove(initialState);

        // Assert
        afterRedo.Should().Be(initialState);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 3)]
    [InlineData(5, 5)]
    [Trait("Type", "Logic")]
    public void MakeMove_ValidPosition_PlacesPlayerPiece(int row, int col)
    {
        // Arrange
        var players = GenerateRandomPlayers();
        var initialState = _service.CreateInitialState(PlayerType.X, players);

        // Act
        var afterMove = _service.MakeMove(initialState, row, col);

        // Assert
        afterMove.Board[row, col].Should().Be(PlayerType.X);
        afterMove.CurrentPlayer.Should().Be(PlayerType.O);
        afterMove.MoveHistory.Should().ContainSingle();
    }

    private Player[] GenerateRandomPlayers()
    {
        var playerFaker = new Faker<Player>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Name, f => f.Person.FullName)
            .RuleFor(p => p.Stats, f => new PoTicTac.Client.Models.PlayerStats
            {
                Wins = f.Random.Int(0, 100),
                Losses = f.Random.Int(0, 100),
                Draws = f.Random.Int(0, 50),
                TotalGames = f.Random.Int(0, 250)
            });

        return playerFaker.Generate(2).ToArray();
    }
}
