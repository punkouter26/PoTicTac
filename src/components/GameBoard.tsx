import React from 'react';
import { GameState } from '../types/GameTypes';
import './GameBoard.css';

interface GameBoardProps {
    gameState: GameState;
    onCellClick: (row: number, col: number) => void;
    lastMove?: { row: number, col: number };
}

export const GameBoard: React.FC<GameBoardProps> = ({ 
    gameState, 
    onCellClick, 
    lastMove 
}) => {
    const isWinningCell = (row: number, col: number) => {
        return gameState.winningCells?.some(([r, c]) => r === row && c === col);
    };

    const getCellStatus = (row: number, col: number) => {
        const cell = gameState.board[row][col];
        const classes = ['cell'];

        if (cell) classes.push('occupied');
        if (cell === 1) classes.push('x-move');
        if (cell === 2) classes.push('o-move');
        if (lastMove?.row === row && lastMove?.col === col) classes.push('last-move');
        if (isWinningCell(row, col)) classes.push('winning');

        return classes.join(' ');
    };

    const isCellDisabled = (row: number, col: number) => {
        return (
            gameState.board[row][col] !== 0 || 
            gameState.gameStatus !== 'playing'
        );
    };

    return (
        <div className={`game-board-container ${gameState.gameStatus}`}>
            <div className="game-board retro-glow">
                {gameState.board.map((row, rowIndex) => (
                    <div key={rowIndex} className="board-row">
                        {row.map((cell, colIndex) => (
                            <button
                                key={`${rowIndex}-${colIndex}`}
                                className={getCellStatus(rowIndex, colIndex)}
                                onClick={() => onCellClick(rowIndex, colIndex)}
                                disabled={isCellDisabled(rowIndex, colIndex)}
                                aria-label={`Cell ${rowIndex}-${colIndex}, ${cell === 1 ? 'X' : cell === 2 ? 'O' : 'empty'}`}
                            >
                                {cell === 1 ? 'X' : cell === 2 ? 'O' : ''}
                            </button>
                        ))}
                    </div>
                ))}
            </div>
        </div>
    );
};
