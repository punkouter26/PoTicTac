import React, { useState } from 'react';
import { DifficultySelector } from './components/DifficultySelector';
import { GameBoard } from './components/GameBoard';
import { GameState, Player } from './types/GameTypes';
import { createInitialState, makeMove } from './utils/gameLogic';
import { getAiMove, Difficulty } from './utils/aiLogic';
import { createInitialStats } from './utils/statsManager';
import './App.css';
import './styles/GameStatus.css';
import Game from './components/Game';

type GameMode = 'menu' | 'singleplayer';

interface GameScore {
    player: number;
    ai: number;
}

export const App: React.FC = () => {
    const [gameMode, setGameMode] = useState<GameMode>('menu');
    const [gameState, setGameState] = useState<GameState | null>(null);
    const [lastMove, setLastMove] = useState<{ row: number; col: number; } | undefined>();
    const [difficulty, setDifficulty] = useState<Difficulty>('medium');
    const [gameScore, setGameScore] = useState<GameScore>({ player: 0, ai: 0 });

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
            { id: '1', name: 'Player 1', type: 'human' as const, symbol: 1 as const, stats: createInitialStats() },
            { id: '2', name: 'AI Player', type: 'ai' as const, symbol: 2 as const, aiConfig: { difficulty }, stats: createInitialStats() }
        ];
        setGameState(createInitialState(1, players));
        setGameMode('singleplayer');
    };

    const returnToMenu = () => {
        setGameState(null);
        setLastMove(undefined);
        setGameMode('menu');
    };

    const updateScore = (winner: number) => {
        setGameScore(prev => ({
            player: winner === 1 ? prev.player + 1 : prev.player,
            ai: winner === 2 ? prev.ai + 1 : prev.ai
        }));
    };

    const getStatusClass = () => {
        if (!gameState || gameState.gameStatus !== 'won') return '';
        const winner = gameState.players.find(p => p.symbol === gameState.winner);
        if (winner?.type === 'ai') return 'ai-wins';
        if (winner?.type === 'human') return 'player-wins';
        return '';
    };

    return (
        <div className="app">
            {gameMode === 'menu' ? (
                <>
                    <h1 style={{
                        fontSize: '3.5em',
                        color: '#00ff00',
                        textShadow: '0 0 10px rgba(0, 255, 0, 0.7)',
                        marginBottom: '30px',
                        fontFamily: 'monospace',
                        letterSpacing: '3px',
                        textTransform: 'uppercase'
                    }}>
                        PoTicTac
                    </h1>
                    <div className="menu">
                        <DifficultySelector
                            currentDifficulty={difficulty}
                            onDifficultyChange={setDifficulty}
                        />
                        <button className="start-button" onClick={startGame}>
                            Start Game
                        </button>
                    </div>
                </>
            ) : gameState && (
                <>
                    <h1>Tic Tac Toe (4 in a row)</h1>
                    {gameState.gameStatus === 'won' && (
                        <div className={`game-status-overlay ${getStatusClass()}`}>
                            {getGameStatus()}
                        </div>
                    )}
                    <div className="game-status" style={{
                        visibility: gameState.gameStatus === 'won' ? 'hidden' : 'visible'
                    }}>
                        {getGameStatus()}
                    </div>
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
