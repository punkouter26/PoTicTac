import React, { FC } from 'react';
import { GameState } from '../types/GameTypes';
import './GameBoard.css';

interface GameBoardProps {
    gameState: GameState;
    onCellClick: (row: number, col: number) => void;
    lastMove?: { row: number; col: number; };
}

export const GameBoard: FC<GameBoardProps> = ({ gameState, onCellClick, lastMove }) => {
    const renderCell = (row: number, col: number) => {
        const value = gameState.board[row][col];
        const isLastMove = lastMove?.row === row && lastMove?.col === col;
        const isWinningCell = gameState.winningCells?.some(([r, c]) => r === row && c === col);
        
        return (
            <div
                key={`${row}-${col}`}
                className={`cell ${isLastMove ? 'last-move' : ''} ${isWinningCell ? 'winning' : ''} ${value ? 'occupied' : ''} ${value === 1 ? 'x-move' : value === 2 ? 'o-move' : ''}`}
                onClick={() => !value && onCellClick(row, col)}
            >
                {value === 1 ? 'X' : value === 2 ? 'O' : ''}
            </div>
        );
    };

    return (
        <div className="game-board-container">
            <div className="game-board retro-glow">
                {gameState.board.map((row, rowIndex) => (
                    <div key={rowIndex} className="board-row">
                        {row.map((_, colIndex) => renderCell(rowIndex, colIndex))}
                    </div>
                ))}
            </div>
        </div>
    );
};
