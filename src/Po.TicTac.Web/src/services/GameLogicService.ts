import {
  GameBoardState,
  PlayerType,
  GameStatus,
  Move,
  Player,
  BOARD_SIZE,
  WIN_LENGTH,
  createEmptyBoard,
} from '../types';

/**
 * Game Logic Service - Singleton Pattern
 * Handles all game state management and move validation.
 * Implements Strategy pattern for win checking directions.
 */
class GameLogicServiceClass {
  /**
   * Creates initial game state with empty board
   * Factory Method pattern for state creation
   */
  createInitialState(
    startingPlayer: PlayerType,
    players: [Player, Player]
  ): GameBoardState {
    return {
      board: createEmptyBoard(),
      currentPlayer: startingPlayer,
      gameStatus: GameStatus.Playing,
      winner: null,
      players,
      moveHistory: [],
      winningCells: null,
    };
  }

  /**
   * Makes a move on the board
   * Returns new state (immutable pattern)
   */
  makeMove(state: GameBoardState, row: number, col: number): GameBoardState {
    // Validate move
    if (
      state.gameStatus !== GameStatus.Playing ||
      state.board[row][col] !== PlayerType.None
    ) {
      return state;
    }

    // Create new board (immutable update)
    const newBoard = state.board.map((r, ri) =>
      r.map((c, ci) => (ri === row && ci === col ? state.currentPlayer : c))
    );

    // Create new move record
    const newMove: Move = {
      player: state.currentPlayer,
      position: [row, col],
      timestamp: Date.now(),
    };

    // Check for winner
    const winResult = this.checkWinner(newBoard);
    const nextPlayer =
      state.currentPlayer === PlayerType.X ? PlayerType.O : PlayerType.X;

    return {
      ...state,
      board: newBoard,
      currentPlayer:
        winResult.winner !== null ? state.currentPlayer : nextPlayer,
      gameStatus: winResult.status,
      winner: winResult.winner,
      moveHistory: [...state.moveHistory, newMove],
      winningCells: winResult.winningCells,
    };
  }

  /**
   * Checks for a winner on the board
   * Uses directional scanning strategy
   */
  checkWinner(board: PlayerType[][]): {
    winner: PlayerType | null;
    status: GameStatus;
    winningCells: [number, number][] | null;
  } {
    // Direction vectors: [rowDelta, colDelta]
    const directions: [number, number][] = [
      [0, 1], // Horizontal
      [1, 0], // Vertical
      [1, 1], // Diagonal down-right
      [-1, 1], // Diagonal up-right
    ];

    for (let row = 0; row < BOARD_SIZE; row++) {
      for (let col = 0; col < BOARD_SIZE; col++) {
        const cell = board[row][col];
        if (cell === PlayerType.None) continue;

        for (const [dr, dc] of directions) {
          const winCells = this.checkLine(board, row, col, dr, dc, cell);
          if (winCells) {
            return {
              winner: cell,
              status: GameStatus.Won,
              winningCells: winCells,
            };
          }
        }
      }
    }

    // Check for draw (no empty cells)
    const hasEmpty = board.some((row) =>
      row.some((cell) => cell === PlayerType.None)
    );
    if (!hasEmpty) {
      return { winner: null, status: GameStatus.Draw, winningCells: null };
    }

    return { winner: null, status: GameStatus.Playing, winningCells: null };
  }

  /**
   * Checks a line in a specific direction for winning sequence
   */
  private checkLine(
    board: PlayerType[][],
    startRow: number,
    startCol: number,
    rowDelta: number,
    colDelta: number,
    player: PlayerType
  ): [number, number][] | null {
    const cells: [number, number][] = [[startRow, startCol]];

    for (let i = 1; i < WIN_LENGTH; i++) {
      const newRow = startRow + i * rowDelta;
      const newCol = startCol + i * colDelta;

      // Check bounds
      if (
        newRow < 0 ||
        newRow >= BOARD_SIZE ||
        newCol < 0 ||
        newCol >= BOARD_SIZE
      ) {
        return null;
      }

      if (board[newRow][newCol] !== player) {
        return null;
      }

      cells.push([newRow, newCol]);
    }

    return cells;
  }

  /**
   * Gets available moves on the board
   */
  getAvailableMoves(board: PlayerType[][]): [number, number][] {
    const moves: [number, number][] = [];
    for (let row = 0; row < BOARD_SIZE; row++) {
      for (let col = 0; col < BOARD_SIZE; col++) {
        if (board[row][col] === PlayerType.None) {
          moves.push([row, col]);
        }
      }
    }
    return moves;
  }
}

// Singleton export
export const GameLogicService = new GameLogicServiceClass();
