import { GameState, Player, Move } from '../types/GameTypes';
import { memoize } from 'lodash';

export const BOARD_SIZE = 6;
export const WIN_LENGTH = 4;
export const TURN_TIME_LIMIT = 5000; // 5 seconds in milliseconds

export const createInitialState = (startingPlayer: 1 | 2, players: [Player, Player]): GameState => {
    return {
        board: Array(BOARD_SIZE).fill(null).map(() => Array(BOARD_SIZE).fill(0)),
        currentPlayer: startingPlayer,
        gameStatus: 'playing',
        winner: null,
        players,
        moveHistory: [],
        undoStack: [],
        redoStack: [],
        sessionStats: {
            totalGames: 0,
            startTime: Date.now(),
            lastUpdateTime: Date.now()
        }
    };
};

export const isValidPosition = (row: number, col: number): boolean => {
    return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
};

export const checkWinner = (board: number[][], lastMove: [number, number]): Array<[number, number]> | null => {
    const [row, col] = lastMove;
    const player = board[row][col];
    const directions = [[0, 1], [1, 0], [1, 1], [1, -1]];
    
    for (const [dx, dy] of directions) {
        const line: Array<[number, number]> = [[row, col]];
        
        // Check in positive direction
        for (let i = 1; i < WIN_LENGTH; i++) {
            const newRow = row + dx * i;
            const newCol = col + dy * i;
            if (!isValidPosition(newRow, newCol) || board[newRow][newCol] !== player) break;
            line.push([newRow, newCol]);
        }
        
        // Check in negative direction
        for (let i = 1; i < WIN_LENGTH; i++) {
            const newRow = row - dx * i;
            const newCol = col - dy * i;
            if (!isValidPosition(newRow, newCol) || board[newRow][newCol] !== player) break;
            line.push([newRow, newCol]);
        }
        
        if (line.length >= WIN_LENGTH) return line;
    }
    return null;
};

// Add memoization for win checking
const memoizedCheckWinner = memoize((board: number[][], lastMove: [number, number]) => {
    return checkWinner(board, lastMove);
});

export const makeMove = (state: GameState, row: number, col: number): GameState => {
    // If game is not in playing state or cell is already occupied
    if (state.gameStatus !== 'playing' || state.board[row][col] !== 0) {
        return state;
    }

    // Create new board with the move
    const newBoard = state.board.map(r => [...r]);
    newBoard[row][col] = state.currentPlayer;

    // Add move to history
    const newMoveHistory = [
        ...(state.moveHistory || []),
        {
            player: state.currentPlayer,
            position: [row, col] as [number, number],
            timestamp: Date.now()
        }
    ];

    // Check for win
    const winningCells = checkWinner(newBoard, [row, col]);
    if (winningCells) {
        return {
            ...state,
            board: newBoard,
            gameStatus: 'won',
            winner: state.currentPlayer,
            winningCells,
            currentPlayer: state.currentPlayer === 1 ? 2 : 1,
            moveHistory: newMoveHistory
        };
    }

    // Check for draw
    if (isDraw(newBoard)) {
        return {
            ...state,
            board: newBoard,
            gameStatus: 'draw',
            currentPlayer: state.currentPlayer === 1 ? 2 : 1,
            moveHistory: newMoveHistory
        };
    }

    // Continue game
    return {
        ...state,
        board: newBoard,
        currentPlayer: state.currentPlayer === 1 ? 2 : 1,
        moveHistory: newMoveHistory
    };
};

const isDraw = (board: number[][]): boolean => {
    return board.every(row => row.every(cell => cell !== 0));
};

// Add board state caching
const boardStateCache = new Map<string, number>();

export const undoMove = (state: GameState): GameState => {
    // If no moves to undo or game is won/drawn, return current state
    if (state.moveHistory.length === 0 || state.gameStatus !== 'playing') {
        return state;
    }

    // Get the last move
    const lastMove = state.moveHistory[state.moveHistory.length - 1];
    const [row, col] = lastMove.position;

    // Create new board with the move undone
    const newBoard = state.board.map(r => [...r]);
    newBoard[row][col] = 0;

    // Update move history and undo/redo stacks
    const newMoveHistory = state.moveHistory.slice(0, -1);
    const newUndoStack = [...state.undoStack, lastMove];

    return {
        ...state,
        board: newBoard,
        currentPlayer: lastMove.player, // Switch back to the player who made the move
        moveHistory: newMoveHistory,
        undoStack: newUndoStack,
        redoStack: []
    };
};

export const redoMove = (state: GameState): GameState => {
    // If no moves to redo or game is won/drawn, return current state
    if (state.undoStack.length === 0 || state.gameStatus !== 'playing') {
        return state;
    }

    // Get the last undone move
    const moveToRedo = state.undoStack[state.undoStack.length - 1];
    const [row, col] = moveToRedo.position;

    // Create new board with the move redone
    const newBoard = state.board.map(r => [...r]);
    newBoard[row][col] = moveToRedo.player;

    // Update move history and undo/redo stacks
    const newMoveHistory = [...state.moveHistory, moveToRedo];
    const newUndoStack = state.undoStack.slice(0, -1);

    // Check for win after redoing the move
    const winningCells = checkWinner(newBoard, [row, col]);
    if (winningCells) {
        return {
            ...state,
            board: newBoard,
            gameStatus: 'won',
            winner: moveToRedo.player,
            winningCells,
            currentPlayer: moveToRedo.player === 1 ? 2 : 1,
            moveHistory: newMoveHistory,
            undoStack: newUndoStack,
            redoStack: []
        };
    }

    // Check for draw
    if (isDraw(newBoard)) {
        return {
            ...state,
            board: newBoard,
            gameStatus: 'draw',
            currentPlayer: moveToRedo.player === 1 ? 2 : 1,
            moveHistory: newMoveHistory,
            undoStack: newUndoStack,
            redoStack: []
        };
    }

    return {
        ...state,
        board: newBoard,
        currentPlayer: moveToRedo.player === 1 ? 2 : 1,
        moveHistory: newMoveHistory,
        undoStack: newUndoStack,
        redoStack: []
    };
};
