import React, { useState, useEffect } from 'react';
import { DifficultySelector } from './components/DifficultySelector';
import { GameBoard } from './components/GameBoard';
import { GameState, Player } from './types/GameTypes';
import { createInitialState, makeMove } from './utils/gameLogic';
import { getAiMove, Difficulty } from './utils/aiLogic';
import { createInitialStats, updatePlayerStats, analyzePerformance } from './utils/statsManager';
import signalRService from './utils/signalRService'; // Updated to use SignalR
import './App.css';
import './styles/GameStatus.css';
import './styles/enhancedAnimations.css';
import './styles/mobileResponsive.css';
import Game from './components/Game';
import GameLobby from './components/GameLobby';
import TouchControls from './components/TouchControls';
import { Statistics } from './components/Statistics';
import { GameStats, PerformanceAnalytics } from './types/GameStats';
import * as signalR from '@microsoft/signalr'; // Import SignalR types

type GameMode = 'menu' | 'singleplayer' | 'multiplayer' | 'multiplayer-game';

interface GameScore {
    player: number;
    ai: number;
}

interface MultiplayerGameInfo {
    gameId: string;
    isHost: boolean;
}

export const App: React.FC = () => {
    const [gameMode, setGameMode] = useState<GameMode>('menu');
    const [gameState, setGameState] = useState<GameState | null>(null);
    const [lastMove, setLastMove] = useState<{ row: number; col: number; } | undefined>();
    const [difficulty, setDifficulty] = useState<Difficulty>('medium');
    const [gameScore, setGameScore] = useState<GameScore>({ player: 0, ai: 0 });
    const [playerName, setPlayerName] = useState<string>(() => {
        const savedName = localStorage.getItem('playerName');
        return savedName || '';
    });
    const [multiplayerGame, setMultiplayerGame] = useState<MultiplayerGameInfo | null>(null);
    const [playerStats, setPlayerStats] = useState<GameStats>(createInitialStats());
    const [showStats, setShowStats] = useState(false);
    const [analytics, setAnalytics] = useState<PerformanceAnalytics>({
        strengths: [],
        weaknesses: [],
        improvementSuggestions: [],
        skillLevel: 'beginner'
    });

    // Connect SignalR on mount or when entering multiplayer
    useEffect(() => {
        if (gameMode === 'multiplayer' || gameMode === 'multiplayer-game') {
            const connection = signalRService.connect();

            // Setup listeners needed in App.tsx
            signalRService.on('MoveReceived', (newGameState: GameState, lastMove: any) => {
                console.log('Received MoveReceived:', newGameState, lastMove);
                setGameState(newGameState);
                
                // Extract move coordinates from the lastMove
                if (lastMove && lastMove.Position) {
                    setLastMove({ row: lastMove.Position[0], col: lastMove.Position[1] });
                } else {
                    setLastMove(undefined);
                }
            });

            signalRService.on('GameOver', (finalGameState: GameState) => {
                console.log('Received GameOver:', finalGameState);
                setGameState(finalGameState);
                if (finalGameState.winner !== null || finalGameState['Winner'] !== null) {
                    // Handle score update if needed
                }
            });

            signalRService.on('RematchRequested', (requesterId: string) => {
                console.log('Rematch requested by:', requesterId);
                // Show a notification or dialog to accept rematch
                if (window.confirm('Your opponent has requested a rematch. Accept?')) {
                    if (multiplayerGame) {
                        signalRService.emit('AcceptRematch', { gameId: multiplayerGame.gameId });
                    }
                }
            });

            signalRService.on('RematchAccepted', (newGameState: GameState) => {
                console.log('Rematch accepted, new game state:', newGameState);
                setGameState(newGameState);
                setLastMove(undefined);
            });

            signalRService.on('PlayerLeft', (playerId: string) => {
                console.log('Player left:', playerId);
                // Notify the user that the opponent left
                alert('Your opponent has left the game.');
            });

            // Start the connection if not connected
            if (connection.state !== signalR.HubConnectionState.Connected) {
                connection.start().catch(err => console.error('Error connecting to SignalR hub:', err));
            }

            // Cleanup listeners on mode change or unmount
            return () => {
                signalRService.off('MoveReceived');
                signalRService.off('GameOver');
                signalRService.off('RematchRequested');
                signalRService.off('RematchAccepted');
                signalRService.off('PlayerLeft');
            };
        } else {
            // Disconnect if not in multiplayer mode
            signalRService.disconnect();
        }
    }, [gameMode, multiplayerGame]);

    // Update stats when game ends
    useEffect(() => {
        if (gameState && (gameState.gameStatus === 'won' || gameState.gameStatus === 'draw')) {
            const newStats = updatePlayerStats(
                playerStats,
                gameState,
                gameState.moveHistory.length,
                1 // Player symbol
            );
            setPlayerStats(newStats);
            setAnalytics(analyzePerformance(newStats));
        }
    }, [gameState?.gameStatus]);

    const handleCellClick = async (row: number, col: number) => {
        if (!gameState || gameState.gameStatus !== 'playing') {
            return;
        }

        // In multiplayer mode, only allow moves on your turn
        if (gameMode === 'multiplayer-game') {
            if (!multiplayerGame || !gameState) return;
            
            try {
                console.log(`Making move: row=${row}, col=${col}, gameId=${multiplayerGame.gameId}`);
                // SignalR method call with parameters in the correct order
                await signalRService.emit('MakeMove', { 
                    gameId: multiplayerGame.gameId,
                    row: row,
                    col: col
                });
                // The game state will be updated via the 'MoveReceived' event handler
            } catch (err) {
                console.error('Error making move:', err);
                // Optionally show error to user
            }
            return;
        }

        // Single player mode
        if (gameState.currentPlayer !== 1) {
            return;
        }

        const newGameState = makeMove(gameState, row, col);
        if (newGameState === gameState) {
            return;
        }

        setGameState(newGameState);
        setLastMove({ row, col });

        // AI move in single player mode
        if (newGameState.gameStatus === 'playing' && newGameState.currentPlayer === 2) { // Ensure it's AI's turn
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
            if (gameMode === 'multiplayer-game') {
                return `${winner?.name} Wins!`;
            }
            return `${winner?.type === 'human' ? 'Player' : 'AI'} Wins!`;
        }
        if (gameState.gameStatus === 'draw') return 'Draw!';
        
        if (gameMode === 'multiplayer-game') {
            const currentPlayer = gameState.players.find(p => p.symbol === gameState.currentPlayer);
            return `${currentPlayer?.name}'s Turn`;
        }
        return `${gameState.currentPlayer === 1 ? 'Player' : 'AI'}'s Turn`;
    };

    const startSinglePlayerGame = () => {
        const players: [Player, Player] = [
            { id: '1', name: playerName || 'Player 1', type: 'human' as const, symbol: 1 as const, stats: createInitialStats() },
            { id: '2', name: 'AI Player', type: 'ai' as const, symbol: 2 as const, aiConfig: { difficulty }, stats: createInitialStats() }
        ];
        setGameState(createInitialState(1, players));
        setGameMode('singleplayer');
    };

    const startMultiplayerLobby = () => {
        setGameState(null);
        setLastMove(undefined);
        setGameMode('multiplayer');
        signalRService.connect(); // Ensure connection for lobby
    };

    const handleMultiplayerGameStart = (gameData: { gameId: string; isHost: boolean; initialState: GameState }) => {
        console.log("Handling multiplayer game start:", gameData);
        setMultiplayerGame({ gameId: gameData.gameId, isHost: gameData.isHost });
        setGameState(gameData.initialState); // Set initial state received from server/lobby
        setGameMode('multiplayer-game');
    };

    const returnToMenu = async () => {
        if (gameMode === 'multiplayer-game' && multiplayerGame) {
            try {
                await signalRService.emit('LeaveGame', { gameId: multiplayerGame.gameId });
            } catch (err) {
                console.error('Error leaving game:', err);
            }
        }
        
        setGameState(null);
        setLastMove(undefined);
        setMultiplayerGame(null);
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
        
        if (gameMode === 'multiplayer-game') {
            return 'player-wins';
        }
        
        const winner = gameState.players.find(p => p.symbol === gameState.winner);
        if (winner?.type === 'ai') return 'ai-wins';
        if (winner?.type === 'human') return 'player-wins';
        return '';
    };

    useEffect(() => {
        if (playerName) {
            localStorage.setItem('playerName', playerName);
        }
    }, [playerName]);

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
                        <div className="player-name-input">
                            <input 
                                type="text" 
                                placeholder="Enter Your Name"
                                value={playerName}
                                onChange={(e) => setPlayerName(e.target.value)}
                                className="name-input"
                            />
                        </div>
                        
                        <div className="menu-options">
                            <div className="mode-selection">
                                <h3>Choose Game Mode</h3>
                                <div className="mode-buttons">
                                    <button className="mode-button" onClick={startSinglePlayerGame}>
                                        <span className="mode-icon">🤖</span>
                                        Single Player
                                    </button>
                                    <button className="mode-button" onClick={startMultiplayerLobby}>
                                        <span className="mode-icon">👥</span>
                                        Multiplayer
                                    </button>
                                </div>
                            </div>
                            
                            <DifficultySelector
                                currentDifficulty={difficulty}
                                onDifficultyChange={setDifficulty}
                            />
                        </div>
                        <button 
                            className="stats-button"
                            onClick={() => setShowStats(true)}
                        >
                            View Statistics
                        </button>
                    </div>
                    {showStats && (
                        <div className="stats-modal">
                            <div className="stats-modal-content">
                                <button 
                                    className="close-stats"
                                    onClick={() => setShowStats(false)}
                                >
                                    ×
                                </button>
                                <Statistics stats={playerStats} analytics={analytics} />
                            </div>
                        </div>
                    )}
                </>
            ) : gameMode === 'multiplayer' ? (
                <GameLobby 
                    playerName={playerName}
                    onGameStart={handleMultiplayerGameStart}
                    onPlayerNameChange={setPlayerName}
                />
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
                    <div className="game-controls">
                        <button onClick={returnToMenu} className="menu-button">
                            Back to Menu
                        </button>
                        {gameMode === 'multiplayer-game' && gameState.gameStatus !== 'playing' && multiplayerGame && (
                            <button 
                                onClick={async () => {
                                    try {
                                        await signalRService.emit('RequestRematch', { gameId: multiplayerGame.gameId });
                                        console.log('Rematch requested');
                                    } catch (err) {
                                        console.error('Error requesting rematch:', err);
                                    }
                                }} 
                                className="rematch-button"
                            >
                                Request Rematch
                            </button>
                        )}
                    </div>
                </>
            )}
        </div>
    );
};
