import React, { useState } from 'react';
import { DifficultySelector } from './components/DifficultySelector';
import { GameBoard } from './components/GameBoard';
import { GameState, Player } from './types/GameTypes';
import { createInitialState, makeMove } from './utils/gameLogic';
import { getAiMove, Difficulty } from './utils/aiLogic';
import './App.css';

type GameMode = 'menu' | 'singleplayer';

export const App: React.FC = () => {
    const [gameMode, setGameMode] = useState<GameMode>('menu');
    const [gameState, setGameState] = useState<GameState | null>(null);
    const [lastMove, setLastMove] = useState<{ row: number; col: number; } | undefined>();
    const [difficulty, setDifficulty] = useState<Difficulty>('medium');

    const handleCellClick = async (row: number, col: number) => {
        if (!gameState || gameState.gameStatus !== 'playing' || gameState.currentPlayer !== 1) {
            return;
        }

        const newGameState = makeMove(gameState, row, col);
        if (newGameState === gameState) {
            return;
        }

        setGameState(newGameState);
        setLastMove({ row, col });

        // AI move
        if (newGameState.gameStatus === 'playing') {
            const makeAiMove = async () => {
                const aiMove = await getAiMove(newGameState, difficulty);
                const afterAiMove = makeMove(newGameState, aiMove[0], aiMove[1]);
                setGameState(afterAiMove);
                setLastMove({ row: aiMove[0], col: aiMove[1] });
            };
            
            setTimeout(() => {
                makeAiMove();
            }, 500);
        }
    };

    const getGameStatus = () => {
        if (!gameState) return '';
        if (gameState.gameStatus === 'won') {
            const winner = gameState.players.find(p => p.symbol === gameState.winner);
            return `${winner?.type === 'human' ? 'Player' : 'AI'} Wins!`;
        }
        if (gameState.gameStatus === 'draw') return 'Draw!';
        return `${gameState.currentPlayer === 1 ? 'Player' : 'AI'}'s Turn`;
    };

    const startGame = () => {
        const players: [Player, Player] = [
            { id: '1', type: 'human', symbol: 1 },
            { id: '2', type: 'ai', symbol: 2, aiConfig: { difficulty } }
        ];
        setGameState(createInitialState(1, players));
        setGameMode('singleplayer');
    };

    const returnToMenu = () => {
        setGameState(null);
        setLastMove(undefined);
        setGameMode('menu');
    };

    return (
        <div className="app">
            <h1>Tic Tac Toe (4 in a row)</h1>
            {gameMode === 'menu' ? (
                <div className="menu">
                    <DifficultySelector
                        currentDifficulty={difficulty}
                        onDifficultyChange={setDifficulty}
                    />
                    <button className="start-button" onClick={startGame}>
                        Start Game
                    </button>
                </div>
            ) : gameState && (
                <>
                    <div className="game-status">{getGameStatus()}</div>
                    <GameBoard
                        gameState={gameState}
                        onCellClick={handleCellClick}
                        lastMove={lastMove}
                    />
                    <button onClick={returnToMenu} className="restart-button">
                        Back to Menu
                    </button>
                </>
            )}
        </div>
    );
};
