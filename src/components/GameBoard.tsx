import React from 'react';
import GameCell from './GameCell';
import '../styles/animations.css';
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
    const isWinningCell = (row: number, col: number): boolean => {
        return gameState.winningCells?.some(([r, c]) => r === row && c === col) || false;
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

    // Add keyboard navigation
    const handleKeyPress = (event: React.KeyboardEvent, row: number, col: number) => {
        if (event.key === 'Enter' || event.key === ' ') {
            onCellClick(row, col);
        }
    };

    const boardClasses = [
        'game-board',
        gameState.gameStatus === 'draw' ? 'board-draw' : ''
    ].filter(Boolean).join(' ');

    return (
        <div className={boardClasses}>
            <div style={{
                display: 'grid',
                gridTemplateColumns: `repeat(${gameState.board[0].length}, 1fr)`,
                gap: '8px',
                padding: '16px',
                backgroundColor: '#f1f2f6',
                borderRadius: '12px',
                boxShadow: '0 8px 24px rgba(0, 0, 0, 0.1)'
            }}>
                {gameState.board.map((row, rowIndex) => (
                    row.map((cell, colIndex) => (
                        <GameCell
                            key={`${rowIndex}-${colIndex}`}
                            value={cell}
                            isWinningCell={isWinningCell(rowIndex, colIndex)}
                            onClick={() => onCellClick(rowIndex, colIndex)}
                        />
                    ))
                ))}
            </div>

            {gameState.gameStatus !== 'playing' && (
                <div className="game-over-overlay" style={{
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    backgroundColor: 'rgba(255, 255, 255, 0.9)',
                    padding: '20px 40px',
                    borderRadius: '12px',
                    textAlign: 'center',
                    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
                    zIndex: 10
                }}>
                    <h2 style={{ margin: '0 0 10px 0' }}>
                        {gameState.gameStatus === 'won' 
                            ? `Player ${gameState.winner} Wins!`
                            : "It's a Draw!"}
                    </h2>
                </div>
            )}
        </div>
    );
};
