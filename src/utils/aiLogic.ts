
import { GameState } from '../types/GameTypes';
import { BOARD_SIZE, isValidPosition, checkWinner } from './gameLogic';

export type Difficulty = 'easy' | 'medium' | 'hard';

interface Move {
    row: number;
    col: number;
    score: number;
}

const DEPTH_BY_DIFFICULTY: Record<Difficulty, number> = {
    easy: 1,
    medium: 2,
    hard: 4
};

export const getAiMove = (gameState: GameState, difficulty: Difficulty): Promise<[number, number]> => {
    return new Promise(resolve => {
        // Add small delay for UX
        setTimeout(() => {
            if (difficulty === 'easy') {
                resolve(getRandomMove(gameState.board));
            } else {
                const depth = DEPTH_BY_DIFFICULTY[difficulty];
                const move = getBestMove(gameState, depth);
                resolve([move.row, move.col]);
            }
        }, 500);
    });
};

const getBestMove = (gameState: GameState, depth: number): Move => {
    const player = gameState.currentPlayer;
    let bestMove: Move = { row: -1, col: -1, score: -Infinity };
    
    for (let row = 0; row < BOARD_SIZE; row++) {
        for (let col = 0; col < BOARD_SIZE; col++) {
            if (gameState.board[row][col] === 0) {
                const newBoard = gameState.board.map(row => [...row]);
                newBoard[row][col] = player;
                
                const score = minimax(
                    newBoard,
                    depth - 1,
                    -Infinity,
                    Infinity,
                    false,
                    player,
                    [row, col]
                );
                
                if (score > bestMove.score) {
                    bestMove = { row, col, score };
                }
            }
        }
    }
    
    return bestMove;
};

const minimax = (
    board: number[][],
    depth: number,
    alpha: number,
    beta: number,
    isMaximizing: boolean,
    aiPlayer: number,
    lastMove: [number, number]
): number => {
    // Base cases
    const winner = checkWinner(board, lastMove);
    if (winner) {
        return board[lastMove[0]][lastMove[1]] === aiPlayer ? 1000 + depth : -1000 - depth;
    }
    if (depth === 0) {
        return evaluatePosition(board, aiPlayer);
    }
    if (isBoardFull(board)) {
        return 0;
    }

    const currentPlayer = isMaximizing ? aiPlayer : (aiPlayer === 1 ? 2 : 1);
    const bestScore = isMaximizing ? -Infinity : Infinity;

    for (let row = 0; row < BOARD_SIZE; row++) {
        for (let col = 0; col < BOARD_SIZE; col++) {
            if (board[row][col] === 0) {
                board[row][col] = currentPlayer;
                const score = minimax(
                    board,
                    depth - 1,
                    alpha,
                    beta,
                    !isMaximizing,
                    aiPlayer,
                    [row, col]
                );
                board[row][col] = 0;

                if (isMaximizing) {
                    alpha = Math.max(alpha, score);
                } else {
                    beta = Math.min(beta, score);
                }

                if (beta <= alpha) {
                    break;
                }
            }
        }
    }

    return isMaximizing ? alpha : beta;
};

const evaluatePosition = (board: number[][], player: number): number => {
    let score = 0;
    const directions = [[0, 1], [1, 0], [1, 1], [1, -1]];

    for (let row = 0; row < BOARD_SIZE; row++) {
        for (let col = 0; col < BOARD_SIZE; col++) {
            if (board[row][col] === 0) {
                for (const [dx, dy] of directions) {
                    score += evaluateDirection(board, row, col, dx, dy, player);
                }
            }
        }
    }

    return score;
};

const evaluateDirection = (
    board: number[][],
    row: number,
    col: number,
    dx: number,
    dy: number,
    player: number
): number => {
    let playerCount = 0;
    let opponentCount = 0;

    for (let i = -3; i <= 3; i++) {
        const newRow = row + dx * i;
        const newCol = col + dy * i;
        
        if (!isValidPosition(newRow, newCol)) continue;
        
        const cell = board[newRow][newCol];
        if (cell === player) playerCount++;
        else if (cell !== 0) opponentCount++;
    }

    if (opponentCount === 0) return Math.pow(10, playerCount);
    if (playerCount === 0) return -Math.pow(10, opponentCount);
    return 0;
};

const getRandomMove = (board: number[][]): [number, number] => {
    const availableMoves: [number, number][] = [];
    
    for (let row = 0; row < BOARD_SIZE; row++) {
        for (let col = 0; col < BOARD_SIZE; col++) {
            if (board[row][col] === 0) {
                availableMoves.push([row, col]);
            }
        }
    }
    
    return availableMoves[Math.floor(Math.random() * availableMoves.length)];
};

const isBoardFull = (board: number[][]): boolean => {
    return board.every(row => row.every(cell => cell !== 0));
};