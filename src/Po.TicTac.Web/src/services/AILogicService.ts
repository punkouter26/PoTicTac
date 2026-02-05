import {
  GameBoardState,
  PlayerType,
  Difficulty,
  BOARD_SIZE,
  WIN_LENGTH,
} from '../types';

/**
 * AI Strategy Interface - Strategy Pattern
 * Defines the contract for AI move calculation
 */
interface AIStrategy {
  getMove(board: PlayerType[][]): [number, number];
}

/**
 * Easy AI Strategy
 * Random moves with 30% chance to block player
 */
class EasyAIStrategy implements AIStrategy {
  getMove(board: PlayerType[][]): [number, number] {
    const availableMoves = getAvailableMoves(board);
    
    // Guard: No available moves (board full or game should have ended)
    if (availableMoves.length === 0) {
      console.warn('EasyAI: No available moves - board may be full');
      return [-1, -1]; // Signal no valid move
    }

    // 30% chance to make a smart blocking move
    if (Math.random() < 0.3) {
      const blockMove = findBlockingMove(board, PlayerType.X);
      if (blockMove) return blockMove;
    }

    // Otherwise random move
    return availableMoves[Math.floor(Math.random() * availableMoves.length)];
  }
}

/**
 * Medium AI Strategy
 * Threat detection with move ordering
 */
class MediumAIStrategy implements AIStrategy {
  getMove(board: PlayerType[][]): [number, number] {
    // Check for winning move
    const winMove = findWinningMove(board, PlayerType.O);
    if (winMove) return winMove;

    // Check for blocking move
    const blockMove = findBlockingMove(board, PlayerType.X);
    if (blockMove) return blockMove;

    // Prefer center and adjacent positions
    const orderedMoves = getOrderedMoves(board);
    if (orderedMoves.length > 0) {
      return orderedMoves[0];
    }

    // Fallback to random
    const availableMoves = getAvailableMoves(board);
    return availableMoves[Math.floor(Math.random() * availableMoves.length)];
  }
}

/**
 * Hard AI Strategy
 * Minimax with alpha-beta pruning
 */
class HardAIStrategy implements AIStrategy {
  private static readonly MAX_DEPTH = 4;
  private transpositionTable = new Map<string, number>();

  getMove(board: PlayerType[][]): [number, number] {
    this.transpositionTable.clear();

    // Check for immediate win/block first
    const winMove = findWinningMove(board, PlayerType.O);
    if (winMove) return winMove;

    const blockMove = findBlockingMove(board, PlayerType.X);
    if (blockMove) return blockMove;

    // Minimax for best move
    let bestScore = -Infinity;
    let bestMove: [number, number] | null = null;

    const orderedMoves = getOrderedMoves(board).slice(0, 10); // Limit for performance

    for (const [row, col] of orderedMoves) {
      board[row][col] = PlayerType.O;
      const score = this.minimax(
        board,
        0,
        false,
        -Infinity,
        Infinity
      );
      board[row][col] = PlayerType.None;

      if (score > bestScore) {
        bestScore = score;
        bestMove = [row, col];
      }
    }

    return bestMove ?? getOrderedMoves(board)[0];
  }

  private minimax(
    board: PlayerType[][],
    depth: number,
    isMaximizing: boolean,
    alpha: number,
    beta: number
  ): number {
    // Check terminal states
    const winner = checkBoardWinner(board);
    if (winner === PlayerType.O) return 1000 - depth;
    if (winner === PlayerType.X) return -1000 + depth;
    if (isBoardFull(board)) return 0;
    if (depth >= HardAIStrategy.MAX_DEPTH) {
      return this.evaluateBoard(board);
    }

    const availableMoves = getOrderedMoves(board).slice(0, 8);

    if (isMaximizing) {
      let maxScore = -Infinity;
      for (const [row, col] of availableMoves) {
        board[row][col] = PlayerType.O;
        const score = this.minimax(board, depth + 1, false, alpha, beta);
        board[row][col] = PlayerType.None;
        maxScore = Math.max(maxScore, score);
        alpha = Math.max(alpha, score);
        if (beta <= alpha) break;
      }
      return maxScore;
    } else {
      let minScore = Infinity;
      for (const [row, col] of availableMoves) {
        board[row][col] = PlayerType.X;
        const score = this.minimax(board, depth + 1, true, alpha, beta);
        board[row][col] = PlayerType.None;
        minScore = Math.min(minScore, score);
        beta = Math.min(beta, score);
        if (beta <= alpha) break;
      }
      return minScore;
    }
  }

  /**
   * Heuristic board evaluation
   */
  private evaluateBoard(board: PlayerType[][]): number {
    let score = 0;
    
    // Evaluate all lines
    const directions: [number, number][] = [
      [0, 1], [1, 0], [1, 1], [-1, 1]
    ];

    for (let row = 0; row < BOARD_SIZE; row++) {
      for (let col = 0; col < BOARD_SIZE; col++) {
        for (const [dr, dc] of directions) {
          score += this.evaluateLine(board, row, col, dr, dc);
        }
      }
    }

    return score;
  }

  private evaluateLine(
    board: PlayerType[][],
    startRow: number,
    startCol: number,
    dr: number,
    dc: number
  ): number {
    let oCount = 0;
    let xCount = 0;

    for (let i = 0; i < WIN_LENGTH; i++) {
      const row = startRow + i * dr;
      const col = startCol + i * dc;

      if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) {
        return 0; // Line extends outside board
      }

      if (board[row][col] === PlayerType.O) oCount++;
      else if (board[row][col] === PlayerType.X) xCount++;
    }

    if (oCount > 0 && xCount > 0) return 0; // Mixed line
    if (oCount > 0) return Math.pow(10, oCount);
    if (xCount > 0) return -Math.pow(10, xCount);
    return 0;
  }
}

// Helper functions
function getAvailableMoves(board: PlayerType[][]): [number, number][] {
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

function getOrderedMoves(board: PlayerType[][]): [number, number][] {
  const center = BOARD_SIZE / 2;
  const moves = getAvailableMoves(board);

  // Sort by distance from center (prefer center)
  return moves.sort((a, b) => {
    const distA = Math.abs(a[0] - center) + Math.abs(a[1] - center);
    const distB = Math.abs(b[0] - center) + Math.abs(b[1] - center);
    return distA - distB;
  });
}

function findWinningMove(
  board: PlayerType[][],
  player: PlayerType
): [number, number] | null {
  for (let row = 0; row < BOARD_SIZE; row++) {
    for (let col = 0; col < BOARD_SIZE; col++) {
      if (board[row][col] === PlayerType.None) {
        board[row][col] = player;
        const winner = checkBoardWinner(board);
        board[row][col] = PlayerType.None;
        if (winner === player) {
          return [row, col];
        }
      }
    }
  }
  return null;
}

function findBlockingMove(
  board: PlayerType[][],
  opponent: PlayerType
): [number, number] | null {
  return findWinningMove(board, opponent);
}

function checkBoardWinner(board: PlayerType[][]): PlayerType | null {
  const directions: [number, number][] = [
    [0, 1], [1, 0], [1, 1], [-1, 1]
  ];

  for (let row = 0; row < BOARD_SIZE; row++) {
    for (let col = 0; col < BOARD_SIZE; col++) {
      const cell = board[row][col];
      if (cell === PlayerType.None) continue;

      for (const [dr, dc] of directions) {
        let count = 1;
        for (let i = 1; i < WIN_LENGTH; i++) {
          const newRow = row + i * dr;
          const newCol = col + i * dc;
          if (
            newRow >= 0 &&
            newRow < BOARD_SIZE &&
            newCol >= 0 &&
            newCol < BOARD_SIZE &&
            board[newRow][newCol] === cell
          ) {
            count++;
          } else {
            break;
          }
        }
        if (count >= WIN_LENGTH) return cell;
      }
    }
  }
  return null;
}

function isBoardFull(board: PlayerType[][]): boolean {
  return board.every((row) => row.every((cell) => cell !== PlayerType.None));
}

/**
 * AI Logic Service - Factory Pattern
 * Creates appropriate AI strategy based on difficulty
 */
class AILogicServiceClass {
  private strategies: Map<Difficulty, AIStrategy> = new Map([
    [Difficulty.Easy, new EasyAIStrategy()],
    [Difficulty.Medium, new MediumAIStrategy()],
    [Difficulty.Hard, new HardAIStrategy()],
  ]);

  /**
   * Gets AI move based on current game state and difficulty
   * Returns a promise for async compatibility
   */
  async getAiMove(
    gameState: GameBoardState,
    difficulty: Difficulty
  ): Promise<[number, number]> {
    const strategy = this.strategies.get(difficulty);
    if (!strategy) {
      throw new Error(`Unknown difficulty: ${difficulty}`);
    }

    // Deep clone board for AI manipulation
    const boardClone = gameState.board.map((row) => [...row]);
    return strategy.getMove(boardClone);
  }
}

// Singleton export
export const AILogicService = new AILogicServiceClass();
