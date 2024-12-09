import { GameState, Player } from '../types/GameTypes';

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
        moveHistory: []
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
