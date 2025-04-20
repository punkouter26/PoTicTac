import React, { useState, useEffect } from 'react';
import { GameBoard } from './GameBoard';
import PlayerTurnIndicator from './PlayerTurnIndicator';
import PlayerStats from './PlayerStats';
import { GameState, Player } from '../types/GameTypes';
import { createInitialState, makeMove, undoMove, redoMove } from '../utils/gameLogic';
import { createInitialStats, updatePlayerStats, saveStatsToLocalStorage, loadStatsFromLocalStorage } from '../utils/statsManager';
import '../styles/animations.css';

const Game: React.FC = () => {
    const createDefaultPlayers = (): [Player, Player] => [
        { id: '1', name: 'Player 1', type: 'human', symbol: 1, stats: createInitialStats() },
        { id: '2', name: 'Player 2', type: 'human', symbol: 2, stats: createInitialStats() }
    ];

    const [players, setPlayers] = useState<[Player, Player]>(() => {
        const savedPlayers = loadStatsFromLocalStorage();
        return savedPlayers || createDefaultPlayers();
    });

    const [gameState, setGameState] = useState<GameState>(() => ({
        ...createInitialState(1, players),
        sessionStats: {
            totalGames: 0,
            startTime: Date.now(),
            lastUpdateTime: Date.now()
        }
    }));

    useEffect(() => {
        // Save stats whenever they change
        saveStatsToLocalStorage(players);
    }, [players]);

    const updateStats = (newGameState: GameState) => {
        if (newGameState.gameStatus === 'playing') return;

        const updatedPlayers: [Player, Player] = [...players] as [Player, Player];

        if (newGameState.gameStatus === 'won') {
            // Update winner and loser stats
            const winner = players.find(p => p.symbol === newGameState.winner)!;
            const loser = players.find(p => p.symbol !== newGameState.winner)!;

            const updatedWinnerStats = updatePlayerStats(winner.stats, newGameState, newGameState.moveHistory.length, winner.symbol);
            const updatedLoserStats = updatePlayerStats(loser.stats, newGameState, newGameState.moveHistory.length, loser.symbol);

            updatedPlayers[winner.symbol - 1] = { ...winner, stats: updatedWinnerStats };
            updatedPlayers[loser.symbol - 1] = { ...loser, stats: updatedLoserStats };
        } else if (newGameState.gameStatus === 'draw') {
            // Update both players' stats for a draw
            updatedPlayers[0] = { 
                ...players[0], 
                stats: updatePlayerStats(players[0].stats, newGameState, newGameState.moveHistory.length, players[0].symbol)
            };
            updatedPlayers[1] = {
                ...players[1],
                stats: updatePlayerStats(players[1].stats, newGameState, newGameState.moveHistory.length, players[1].symbol)
            };
        }

        setPlayers(updatedPlayers);
    };

    const handleCellClick = (row: number, col: number) => {
        setGameState(prevState => {
            const newState = makeMove(prevState, row, col);
            if (newState.gameStatus !== 'playing' && newState.gameStatus !== prevState.gameStatus) {
                // Game just ended
                updateStats(newState);
            }
            return newState;
        });
    };

    const handleUndo = () => {
        setGameState(prevState => undoMove(prevState));
    };

    const handleRedo = () => {
        setGameState(prevState => redoMove(prevState));
    };

    const handleNewGame = () => {
        setGameState(prevState => ({
            ...createInitialState(1, players),
            sessionStats: {
                ...prevState.sessionStats,
                totalGames: prevState.sessionStats.totalGames + 1,
                lastUpdateTime: Date.now()
            }
        }));
    };

    return (
        <div style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            padding: '20px',
            minHeight: '100vh',
            backgroundColor: '#f8f9fa'
        }}>
            <h1 style={{
                fontSize: '2.5rem',
                color: '#2f3542',
                marginBottom: '20px',
                textAlign: 'center'
            }}>
                Tic Tac Toe
            </h1>

            <div style={{
                display: 'flex',
                justifyContent: 'center',
                gap: '40px',
                width: '100%',
                maxWidth: '1200px',
                margin: '20px 0'
            }}>
                <PlayerStats player={players[0]} />
                
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <PlayerTurnIndicator
                        currentPlayer={gameState.currentPlayer}
                        players={players}
                    />

                    <div style={{
                        position: 'relative',
                        margin: '20px 0'
                    }}>
                        <GameBoard
                            gameState={gameState}
                            onCellClick={handleCellClick}
                            lastMove={gameState.moveHistory.length > 0 
                                ? { 
                                    row: gameState.moveHistory[gameState.moveHistory.length - 1].position[0],
                                    col: gameState.moveHistory[gameState.moveHistory.length - 1].position[1]
                                  }
                                : undefined}
                        />
                    </div>

                    <div style={{
                        display: 'flex',
                        gap: '12px',
                        marginTop: '20px'
                    }}>
                        <button
                            className="button-hover"
                            onClick={handleUndo}
                            disabled={gameState.moveHistory.length === 0 || gameState.gameStatus !== 'playing'}
                            style={{
                                padding: '10px 20px',
                                backgroundColor: '#2f3542',
                                color: '#fff',
                                border: 'none',
                                borderRadius: '6px',
                                cursor: 'pointer',
                                fontSize: '1rem'
                            }}
                        >
                            Undo
                        </button>

                        <button
                            className="button-hover"
                            onClick={handleRedo}
                            disabled={gameState.undoStack.length === 0 || gameState.gameStatus !== 'playing'}
                            style={{
                                padding: '10px 20px',
                                backgroundColor: '#2f3542',
                                color: '#fff',
                                border: 'none',
                                borderRadius: '6px',
                                cursor: 'pointer',
                                fontSize: '1rem'
                            }}
                        >
                            Redo
                        </button>

                        <button
                            className="button-hover"
                            onClick={handleNewGame}
                            style={{
                                padding: '10px 20px',
                                backgroundColor: '#ff4757',
                                color: '#fff',
                                border: 'none',
                                borderRadius: '6px',
                                cursor: 'pointer',
                                fontSize: '1rem'
                            }}
                        >
                            New Game
                        </button>
                    </div>
                </div>

                <PlayerStats player={players[1]} />
            </div>
        </div>
    );
};

export default Game; 
