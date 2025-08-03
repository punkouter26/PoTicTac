using Xunit;
using PoTicTac.Client.Services;
using PoTicTac.Client.Models;

namespace PoTicTac.UnitTests
{
    public class GameLogicServiceTests
    {
        private readonly GameLogicService _service;

        public GameLogicServiceTests()
        {
            _service = new GameLogicService();
        }

        [Fact]
        public void CheckWinner_EmptyBoard_ReturnsNull()
        {
            var board = new GameBoardState();
            var result = _service.CheckWinner(board);
            Assert.Null(result);
        }

        [Fact]
        public void CheckWinner_HorizontalWin_ReturnsWinner()
        {
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
            var result = _service.CheckWinner(board);
            Assert.Equal(PlayerType.X, result);
        }

        [Fact]
        public void CheckWinner_VerticalWin_ReturnsWinner()
        {
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
            var result = _service.CheckWinner(board);
            Assert.Equal(PlayerType.X, result);
        }

        [Fact]
        public void CheckWinner_DiagonalWin_ReturnsWinner()
        {
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
            var result = _service.CheckWinner(board);
            Assert.Equal(PlayerType.X, result);
        }

        [Fact]
        public void CheckWinner_Draw_ReturnsDraw()
        {
            var board = new GameBoardState
            {
                Board = new[,]
                {
                    { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O },
                    { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X },
                    { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O },
                    { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X },
                    { PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O },
                    { PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X, PlayerType.O, PlayerType.X }
                }
            };
            var result = _service.CheckWinner(board);
            Assert.Equal(PlayerType.Draw, result);
        }

        [Fact]
        public void UndoMove_WithMoveHistory_UndoesLastMove()
        {
            var initialState = _service.CreateInitialState(PlayerType.X, new Player[2]);
            var afterMove = _service.MakeMove(initialState, 0, 0);
            var afterUndo = _service.UndoMove(afterMove);

            Assert.Equal(PlayerType.None, afterUndo.Board[0, 0]);
            Assert.Empty(afterUndo.MoveHistory);
            Assert.Single(afterUndo.UndoStack);
            Assert.Equal(PlayerType.X, afterUndo.CurrentPlayer);
        }

        [Fact]
        public void UndoMove_NoMoves_ReturnsSameState()
        {
            var initialState = _service.CreateInitialState(PlayerType.X, new Player[2]);
            var afterUndo = _service.UndoMove(initialState);

            Assert.Equal(initialState, afterUndo);
        }

        [Fact]
        public void RedoMove_WithUndoneMove_RedoesMove()
        {
            var initialState = _service.CreateInitialState(PlayerType.X, new Player[2]);
            var afterMove = _service.MakeMove(initialState, 0, 0);
            var afterUndo = _service.UndoMove(afterMove);
            var afterRedo = _service.RedoMove(afterUndo);

            Assert.Equal(PlayerType.X, afterRedo.Board[0, 0]);
            Assert.Single(afterRedo.MoveHistory);
            Assert.Empty(afterRedo.UndoStack);
            Assert.Equal(PlayerType.O, afterRedo.CurrentPlayer);
        }

        [Fact]
        public void RedoMove_NoUndoneMoves_ReturnsSameState()
        {
            var initialState = _service.CreateInitialState(PlayerType.X, new Player[2]);
            var afterRedo = _service.RedoMove(initialState);

            Assert.Equal(initialState, afterRedo);
        }
    }
}
