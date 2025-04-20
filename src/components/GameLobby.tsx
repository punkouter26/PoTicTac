import React, { useState, useEffect, useCallback } from 'react';
import signalRService from '../utils/signalRService'; // Updated to use SignalR
import './GameLobby.css';
import { GameState } from '../types/GameTypes'; // Import GameState
import * as signalR from '@microsoft/signalr'; // Import SignalR types

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
  const handleGameJoined = useCallback((gameState: GameState, players: any[]) => {
    console.log("Game joined event received:", gameState);
    onGameStart({
      gameId: gameState.gameId || gameState['GameId'], // Handle both cases
      isHost: players?.[0]?.Id === signalRService.getConnection()?.connectionId,
      initialState: gameState
    });
  }, [onGameStart]);

  const handleGameCreated = useCallback((gameId: string) => {
    console.log("Game created event received:", gameId);
    setGameInvite(gameId);
  }, []);

  // Connect to SignalR server and set up listeners
  useEffect(() => {
    setIsConnecting(true);
    
    // Connect and set up connection state listeners
    const connection = signalRService.connect();

    // Check connection state
    if (connection.state === signalR.HubConnectionState.Connected) {
      setIsConnected(true);
      setIsConnecting(false);
    }

    // Setup event handlers
    connection.onreconnecting(() => {
      console.log("Reconnecting to server...");
      setIsConnecting(true);
      setIsConnected(false);
    });

    connection.onreconnected(() => {
      console.log("Reconnected to server");
      setIsConnected(true);
      setIsConnecting(false);
    });

    connection.onclose(() => {
      console.log("Connection closed");
      setIsConnected(false);
      setIsConnecting(false);
    });

    // Register SignalR method handlers
    signalRService.on('GameCreated', handleGameCreated);
    signalRService.on('GameJoined', handleGameJoined);
    signalRService.on('PlayerJoined', (player) => {
      console.log("Player joined:", player);
    });

    // Start the connection if not already started
    if (connection.state !== signalR.HubConnectionState.Connected) {
      connection.start()
        .then(() => {
          console.log("Connected to SignalR hub");
          setIsConnected(true);
          setIsConnecting(false);
          setError(null);
        })
        .catch(err => {
          console.error("SignalR Connection Error: ", err);
          setError('Failed to connect to game server. Please try again.');
          setIsConnected(false);
          setIsConnecting(false);
        });
    }

    // Clean up listeners on unmount
    return () => {
      console.log("Cleaning up lobby listeners...");
      signalRService.off('GameCreated');
      signalRService.off('GameJoined');
      signalRService.off('PlayerJoined');
    };
  }, [handleGameCreated, handleGameJoined]);

  const handleCreateGame = async () => {
    if (!playerName.trim()) {
      setError('Please enter your name');
      return;
    }
    setError(null);
    console.log("Invoking CreateGame with player name:", playerName);
    
    try {
      await signalRService.ensureConnected();
      // SignalR method invocation is different from Socket.IO events
      await signalRService.emit('CreateGame', { playerName });
    } catch (err) {
      console.error("Error creating game:", err);
      setError('Failed to create game. Please try again.');
    }
  };

  const handleJoinGame = async () => {
    if (!playerName.trim()) {
      setError('Please enter your name');
      return;
    }

    if (!gameCode.trim()) {
      setError('Please enter a game code');
      return;
    }
    
    setError(null);
    console.log(`Invoking JoinGame with code: ${gameCode}, name: ${playerName}`);
    
    try {
      await signalRService.ensureConnected();
      // SignalR method invocation with parameters
      await signalRService.emit('JoinGame', { gameId: gameCode, playerName });
    } catch (err) {
      console.error("Error joining game:", err);
      setError(`Failed to join game. ${err}`);
    }
  };

  // Rest of your component remains the same
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
