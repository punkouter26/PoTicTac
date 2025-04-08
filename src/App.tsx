import React, { useState, useEffect } from 'react';
import { DifficultySelector } from './components/DifficultySelector';
import { GameBoard } from './components/GameBoard';
import { GameState, Player } from './types/GameTypes';
import { createInitialState, makeMove } from './utils/gameLogic';
import { getAiMove, Difficulty } from './utils/aiLogic';
import { createInitialStats } from './utils/statsManager';
import socketService from './utils/socketService'; // Import socketService
import './App.css';
import './styles/GameStatus.css';
import './styles/enhancedAnimations.css';
import './styles/mobileResponsive.css';
import Game from './components/Game';
import GameLobby from './components/GameLobby';
import TouchControls from './components/TouchControls';

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

    // Connect socket on mount or when entering multiplayer
    useEffect(() => {
        if (gameMode === 'multiplayer' || gameMode === 'multiplayer-game') {
            const socket = socketService.connect();

            // Setup listeners needed in App.tsx (e.g., game state updates)
            // Listener now expects an object { gameState: GameState, placedMove: { row: number, col: number } }
            socket.on('game_state_update', (data: { gameState: GameState, placedMove?: { row: number, col: number } }) => {
                console.log('Received game_state_update:', data);
                setGameState(data.gameState);
                // Use the actual placed move coordinates from the server for highlighting
                if (data.placedMove) {
                    setLastMove(data.placedMove);
                } else {
                    // Fallback or clear if no move info provided (e.g., on game start)
                    setLastMove(undefined); 
                }
            });

            socket.on('game_over', (data: { winner: number | null, gameState: GameState }) => {
                console.log('Received game_over:', data);
                setGameState(data.gameState);
                if (data.winner !== null) {
                    // Handle score update if needed
                }
            });

            // Cleanup listeners on mode change or unmount
            return () => {
                socket.off('game_state_update');
                socket.off('game_over');
                // Consider disconnecting if leaving multiplayer entirely
                // socketService.disconnect(); 
            };
        } else {
            // Disconnect if not in multiplayer mode
            socketService.disconnect();
        }
    }, [gameMode]);

    const handleCellClick = async (row: number, col: number) => {
        if (!gameState || gameState.gameStatus !== 'playing') {
            return;
        }

        // In multiplayer mode, only allow moves on your turn
        if (gameMode === 'multiplayer-game') {
            if (!multiplayerGame || !gameState) return;

            // Basic check if it's the player's turn (more robust check might be needed)
            const currentPlayerDetails = gameState.players.find(p => p.symbol === gameState.currentPlayer);
            // Assuming the local player's ID is stored or can be derived
            // This needs refinement - how do we know which player *this* client is?
            // Let's assume player 1 is host, player 2 is guest for now, or pass player ID
            // For now, let's just send the move and let the server validate
            
            console.log(`Sending move: row=${row}, col=${col}, gameId=${multiplayerGame.gameId}`);
            socketService.emit('make_move', { 
                gameId: multiplayerGame.gameId, 
                move: { row, col } 
                // We might need to send playerSymbol or playerId here too
            });
            // The game state will be updated via the 'game_state_update' event from the server
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
    };    const getGameStatus = () => {
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
        socketService.connect(); // Ensure connection for lobby
    };

    // This function might now be triggered by an event from the lobby component
    // or directly if App handles joining/creating games itself.
    // Let's assume the lobby emits an event 'start_game' handled here.
    const handleMultiplayerGameStart = (gameData: { gameId: string; isHost: boolean; initialState: GameState }) => {
        console.log("Handling multiplayer game start:", gameData);
        setMultiplayerGame({ gameId: gameData.gameId, isHost: gameData.isHost });
        setGameState(gameData.initialState); // Set initial state received from server/lobby
        setGameMode('multiplayer-game');
        
        // Listeners for game updates are now in the useEffect hook
    };

    const returnToMenu = () => {
        if (gameMode === 'multiplayer-game' && multiplayerGame) {
            socketService.emit('leave_game', { gameId: multiplayerGame.gameId });
        }
        // Disconnect socket when returning to menu? Or keep alive?
        // socketService.disconnect(); 
        
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

    // Save player name to localStorage when it changes
    useEffect(() => {
        if (playerName) {
            localStorage.setItem('playerName', playerName);
        }
    }, [playerName]);    return (
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
                    </div>
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
                            <button onClick={() => socketService.emit('request_rematch', { gameId: multiplayerGame.gameId })} className="rematch-button">
                                Request Rematch
                            </button>
                        )}
                    </div>
                </>
            )}
        </div>
    );
};
