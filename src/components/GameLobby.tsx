import React, { useState, useEffect, useCallback } from 'react';
import socketService from '../utils/socketService'; // Use socketService
import './GameLobby.css';
import { GameState } from '../types/GameTypes'; // Import GameState

// Define the structure for game data passed to onGameStart
interface GameStartData {
  gameId: string;
  isHost: boolean;
  initialState: GameState;
}

interface GameLobbyProps {
  playerName: string;
  onGameStart: (gameData: GameStartData) => void; // Updated signature
  onPlayerNameChange: (name: string) => void;
}

const GameLobby: React.FC<GameLobbyProps> = ({ 
  playerName, 
  onGameStart, 
  onPlayerNameChange 
}) => {
  const [isConnecting, setIsConnecting] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [gameCode, setGameCode] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [gameInvite, setGameInvite] = useState<string | null>(null);

  // Use useCallback for event handlers passed to useEffect
  const handleGameJoined = useCallback((data: GameStartData) => {
    console.log("Game joined event received:", data);
    onGameStart(data); // Pass the whole data object
  }, [onGameStart]);

  const handleGameCreated = useCallback((data: { gameId: string }) => {
    console.log("Game created event received:", data);
    setGameInvite(data.gameId);
  }, []);

  const handleLobbyError = useCallback((data: { message: string }) => {
    console.error("Lobby error received:", data);
    setError(data.message || 'An error occurred in the lobby.');
  }, []);

  // Connect to Socket.IO server and set up listeners
  useEffect(() => {
    const socket = socketService.connect(); // Ensure connection
    // setIsConnecting(true); // Connection status handled by socket events now

    const onConnect = () => {
      console.log('Lobby socket connected:', socket.id);
      setIsConnected(true);
      setIsConnecting(false);
      setError(null);
    };

    const onDisconnect = (reason: string) => {
      console.log('Lobby socket disconnected:', reason);
      setIsConnected(false);
      setIsConnecting(false);
      // setError('Disconnected from server.'); // Optional: inform user
    };

    const onConnectError = (error: Error) => {
      console.error('Lobby socket connection error:', error);
      setError('Failed to connect to game server. Please try again.');
      setIsConnected(false);
      setIsConnecting(false);
    };

    // Check initial connection status
    if (socket.connected) {
      onConnect();
    } else {
      setIsConnecting(true); // Show connecting state until 'connect' or 'connect_error'
    }

    // Add listeners
    socket.on('connect', onConnect);
    socket.on('disconnect', onDisconnect);
    socket.on('connect_error', onConnectError);
    socket.on('game_created', handleGameCreated); // Listen for game creation confirmation
    socket.on('game_joined', handleGameJoined);   // Listen for successful join (for both players)
    socket.on('lobby_error', handleLobbyError);   // Listen for specific lobby errors

    // Clean up listeners on unmount
    return () => {
      console.log("Cleaning up lobby listeners...");
      socket.off('connect', onConnect);
      socket.off('disconnect', onDisconnect);
      socket.off('connect_error', onConnectError);
      socket.off('game_created', handleGameCreated);
      socket.off('game_joined', handleGameJoined);
      socket.off('lobby_error', handleLobbyError);
      // Decide if socket should be disconnected when leaving lobby
      // socketService.disconnect();
    };
  }, [handleGameCreated, handleGameJoined, handleLobbyError]); // Dependencies for useCallback functions

  const handleCreateGame = () => { // No longer async, just emits
    if (!playerName.trim()) {
      setError('Please enter your name');
      return;
    }
    setError(null);
    console.log("Emitting create_game with player name:", playerName);
    socketService.emit('create_game', { playerName });
    // Game start is now handled by the 'game_joined' event listener for the creator
  };

  const handleJoinGame = () => { // No longer async, just emits
    if (!playerName.trim()) {
      setError('Please enter your name');
      return;
    }

    if (!gameCode.trim()) {
      setError('Please enter a game code');
      return;
    }
    setError(null);
    console.log(`Emitting join_game with code: ${gameCode}, name: ${playerName}`);
    socketService.emit('join_game', { gameCode, playerName });
    // Game start is handled by the 'game_joined' event listener for the joiner
  };

  const copyGameCodeToClipboard = () => {
    if (gameInvite) {
      navigator.clipboard.writeText(gameInvite);
      // Show a temporary message
      const tempElement = document.createElement('div');
      tempElement.innerText = 'Game code copied!';
      tempElement.className = 'copy-notification';
      document.body.appendChild(tempElement);
      setTimeout(() => document.body.removeChild(tempElement), 2000);
    }
  };

  return (
    <div className="game-lobby">
      <h2>Multiplayer Lobby</h2>
      
      <div className="player-input">
        <input
          type="text"
          placeholder="Your Name"
          value={playerName}
          onChange={(e) => onPlayerNameChange(e.target.value)}
          disabled={isConnecting}
        />
      </div>

      {isConnected ? (
        <>
          <div className="lobby-actions">
            <button
              className="create-game-btn"
              onClick={handleCreateGame}
              disabled={isConnecting || !playerName.trim()}
            >
              Create Game
            </button>

            <div className="join-game-section">
              <input
                type="text"
                placeholder="Enter Game Code"
                value={gameCode}
                onChange={(e) => setGameCode(e.target.value)}
                disabled={isConnecting}
              />
              <button
                className="join-game-btn"
                onClick={handleJoinGame}
                disabled={isConnecting || !playerName.trim() || !gameCode.trim()}
              >
                Join Game
              </button>
            </div>
          </div>

          {gameInvite && (
            <div className="game-invite">
              <p>Share this code with your opponent:</p>
              <div className="game-code">{gameInvite}</div>
              <button onClick={copyGameCodeToClipboard}>
                Copy Code
              </button>
            </div>
          )}
        </>
      ) : (
        <div className="connecting-message">
          {isConnecting ? 'Connecting to server...' : 'Disconnected from server'}
        </div>
      )}

      {error && <div className="error-message">{error}</div>}
    </div>
  );
};

export default GameLobby;
