import { GameState } from '../types/GameTypes';

// Weights for different factors that influence win probability
const WEIGHTS = {
    CENTER_CONTROL: 15,
    CORNER_CONTROL: 10,
    BLOCKING_OPPONENT: 20,
    WINNING_OPPORTUNITY: 30,
    OPPONENT_WINNING_OPPORTUNITY: -25,
};

/**
 * Calculates the win probability for the human player based on the current game state
 * @param board The current game board
 * @param humanSymbol The symbol used by the human player (1 for X, -1 for O)
 * @returns Win probability percentage (0-100)
 */
export const calculateWinProbability = (board: number[], humanSymbol: number): number => {
    if (checkWin(board, humanSymbol)) return 100;
    if (checkWin(board, -humanSymbol)) return 0;
    if (board.every(cell => cell !== 0)) return 50; // Draw

    let probability = 50; // Start at 50%

    // Check center control
    if (board[4] === humanSymbol) {
        probability += WEIGHTS.CENTER_CONTROL;
    } else if (board[4] === -humanSymbol) {
        probability -= WEIGHTS.CENTER_CONTROL;
    }

    // Check corner control
    const corners = [0, 2, 6, 8];
    const humanCorners = corners.filter(i => board[i] === humanSymbol).length;
    const opponentCorners = corners.filter(i => board[i] === -humanSymbol).length;
    probability += (humanCorners - opponentCorners) * WEIGHTS.CORNER_CONTROL;

    // Check winning opportunities
    const humanWinningMoves = countWinningMoves(board, humanSymbol);
    const opponentWinningMoves = countWinningMoves(board, -humanSymbol);

    probability += humanWinningMoves * WEIGHTS.WINNING_OPPORTUNITY;
    probability += opponentWinningMoves * WEIGHTS.OPPONENT_WINNING_OPPORTUNITY;

    // Ensure probability stays within 0-100 range
    return Math.max(0, Math.min(100, probability));
};

/**
 * Counts the number of possible winning moves for a player
 */
const countWinningMoves = (board: number[], symbol: number): number => {
    let count = 0;
    for (let i = 0; i < 9; i++) {
        if (board[i] === 0) {
            const tempBoard = [...board];
            tempBoard[i] = symbol;
            if (checkWin(tempBoard, symbol)) count++;
        }
    }
    return count;
};

/**
 * Checks if a player has won
 */
const checkWin = (board: number[], symbol: number): boolean => {
    const winPatterns = [
        [0, 1, 2], [3, 4, 5], [6, 7, 8], // Rows
        [0, 3, 6], [1, 4, 7], [2, 5, 8], // Columns
        [0, 4, 8], [2, 4, 6] // Diagonals
    ];

    return winPatterns.some(pattern =>
        pattern.every(pos => board[pos] === symbol)
    );
};

/**
 * Tracks win probability history throughout a game
 */
export class WinProbabilityTracker {
    private history: number[] = [];
    private currentBoard: number[] = Array(9).fill(0);
    private humanSymbol: number;

    constructor(humanSymbol: number) {
        this.humanSymbol = humanSymbol;
        this.history.push(50); // Initial 50% probability
    }

    /**
     * Updates the probability history with the new board state
     */
    updateProbability(board: number[]): void {
        this.currentBoard = [...board];
        const probability = calculateWinProbability(board, this.humanSymbol);
        this.history.push(probability);
    }

    /**
     * Gets the complete probability history
     */
    getProbabilityHistory(): number[] {
        return this.history;
    }

    /**
     * Gets the current move number
     */
    getCurrentMove(): number {
        return this.history.length - 1;
    }

    /**
     * Resets the probability tracker for a new game
     */
    reset(): void {
        this.history = [50];
        this.currentBoard = Array(9).fill(0);
    }
} 