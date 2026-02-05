import { useState, useCallback, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { GameBoard } from './GameBoard';
import { DifficultySelector } from './DifficultySelector';
import { GameLogicService, AILogicService, StatisticsApi, LocalStorageService } from '../services';
import {
  GameBoardState,
  GameMode,
  GameStatus,
  Difficulty,
  PlayerType,
  Player,
  LastMoveInfo,
  DifficultyStats,
  createDefaultPlayerStats,
} from '../types';
import '../styles/components.css';

/**
 * Game component - main game orchestrator
 * Manages game state and player interactions
 */
export function Game() {
  const navigate = useNavigate();

  // State
  const [gameMode, setGameMode] = useState<GameMode>(GameMode.Menu);
  const [gameState, setGameState] = useState<GameBoardState | null>(null);
  const [lastMove, setLastMove] = useState<LastMoveInfo | null>(null);
  const [difficulty, setDifficulty] = useState<Difficulty>(() => {
    const saved = LocalStorageService.getDifficulty();
    return (saved as Difficulty) || Difficulty.Medium;
  });
  const [playerName, setPlayerName] = useState<string>(() =>
    LocalStorageService.getPlayerName()
  );
  const [selectedMode, setSelectedMode] = useState<string>('');
  const [isStartingGame, setIsStartingGame] = useState(false);
  const [isAiThinking, setIsAiThinking] = useState(false);
  const [statsSaved, setStatsSaved] = useState(false); // Issue 8: Prevent double save
  const moveLockRef = useRef(false); // Issue 3: Synchronous lock to prevent race conditions

  // Save player name when it changes
  useEffect(() => {
    if (playerName) {
      LocalStorageService.setPlayerName(playerName);
    }
  }, [playerName]);

  // Save difficulty when it changes
  useEffect(() => {
    LocalStorageService.setDifficulty(difficulty);
  }, [difficulty]);

  const [nameWarning, setNameWarning] = useState(false);
  const canStartGame = selectedMode !== '';

  // Create players for single player game
  const createPlayers = useCallback((): [Player, Player] => {
    return [
      {
        id: '1',
        name: playerName || 'Player 1',
        type: PlayerType.Human,
        symbol: PlayerType.X,
      },
      {
        id: '2',
        name: 'AI Player',
        type: PlayerType.AI,
        symbol: PlayerType.O,
        aiConfig: { difficulty },
      },
    ];
  }, [playerName, difficulty]);

  // Start single player game
  const startSinglePlayerGame = useCallback(() => {
    console.log('Starting single player game');
    setStatsSaved(false); // Issue 8: Reset save flag for new game
    const players = createPlayers();
    const initialState = GameLogicService.createInitialState(PlayerType.X, players);
    setGameState(initialState);
    setGameMode(GameMode.Singleplayer);
    setLastMove(null);
  }, [createPlayers]);

  // Handle starting selected game mode
  const handleStartGame = useCallback(async () => {
    if (!selectedMode) return;

    // Issue 1 fix: Warn if no player name for stats tracking
    if (!playerName.trim()) {
      setNameWarning(true);
    } else {
      setNameWarning(false);
    }

    setIsStartingGame(true);
    try {
      await new Promise((resolve) => setTimeout(resolve, 300)); // Visual feedback
      if (selectedMode === 'single') {
        startSinglePlayerGame();
      }
    } finally {
      setIsStartingGame(false);
    }
  }, [selectedMode, startSinglePlayerGame]);

  // Save stats after game ends
  const saveStats = useCallback(
    async (finalState: GameBoardState) => {
      // Issue 1: Skip if no valid player name
      if (!playerName || !playerName.trim()) return;
      // Issue 8: Prevent double save
      if (statsSaved) return;
      setStatsSaved(true);

      try {
        // Get existing stats
        const existingDto = await StatisticsApi.getPlayerStats(playerName);
        const stats = existingDto?.stats ?? createDefaultPlayerStats();

        // Get the difficulty stats to update
        let diffStats: DifficultyStats;
        switch (difficulty) {
          case Difficulty.Easy:
            diffStats = stats.easy;
            break;
          case Difficulty.Medium:
            diffStats = stats.medium;
            break;
          case Difficulty.Hard:
            diffStats = stats.hard;
            break;
        }

        // Update stats
        diffStats.totalGames++;

        const playerWon = finalState.gameStatus === GameStatus.Won && finalState.winner === PlayerType.X;
        const aiWon = finalState.gameStatus === GameStatus.Won && finalState.winner === PlayerType.O;
        const isDraw = finalState.gameStatus === GameStatus.Draw;

        if (playerWon) {
          diffStats.wins++;
          diffStats.currentStreak++;
          if (diffStats.currentStreak > diffStats.winStreak) {
            diffStats.winStreak = diffStats.currentStreak;
          }
        } else if (aiWon) {
          diffStats.losses++;
          diffStats.currentStreak = 0; // Reset streak on loss
        } else if (isDraw) {
          diffStats.draws++;
          // Issue 4 fix: Draw does NOT reset win streak - only losses do
        }

        // Update moves
        const playerMoves = finalState.moveHistory.filter((m) => m.player === PlayerType.X).length;
        diffStats.totalMoves += playerMoves;
        diffStats.averageMovesPerGame =
          diffStats.totalGames > 0 ? diffStats.totalMoves / diffStats.totalGames : 0;
        diffStats.winRate = diffStats.totalGames > 0 ? diffStats.wins / diffStats.totalGames : 0;

        // Update overall stats
        stats.totalGames = stats.easy.totalGames + stats.medium.totalGames + stats.hard.totalGames;
        stats.totalWins = stats.easy.wins + stats.medium.wins + stats.hard.wins;
        stats.totalLosses = stats.easy.losses + stats.medium.losses + stats.hard.losses;
        stats.totalDraws = stats.easy.draws + stats.medium.draws + stats.hard.draws;
        stats.overallWinRate = stats.totalGames > 0 ? stats.totalWins / stats.totalGames : 0;

        // Save to local storage (for offline)
        LocalStorageService.setPlayerStats(playerName, stats);

        // Save to backend
        await StatisticsApi.savePlayerStats(playerName, stats);
        console.log('✅ Stats saved successfully');
      } catch (error) {
        console.error('Failed to save stats:', error);
      }
    },
    [playerName, difficulty]
  );

  // Handle cell click
  const handleCellClick = useCallback(
    async (row: number, col: number) => {
      if (!gameState || gameState.gameStatus !== GameStatus.Playing) return;
      if (gameState.currentPlayer !== PlayerType.X) return;
      if (isAiThinking) return;
      
      // Issue 3: Synchronous lock to prevent race conditions from rapid clicks
      if (moveLockRef.current) return;
      moveLockRef.current = true;

      // Make player move
      const newState = GameLogicService.makeMove(gameState, row, col);
      if (newState === gameState) {
        moveLockRef.current = false; // Release lock on invalid move
        return;
      }

      setGameState(newState);
      setLastMove({ row, col });

      // Check if game ended after player move
      if (newState.gameStatus !== GameStatus.Playing) {
        await saveStats(newState);
        moveLockRef.current = false;
        return;
      }

      // AI move
      if (newState.currentPlayer === PlayerType.O) {
        setIsAiThinking(true);
        try {
          await new Promise((resolve) => setTimeout(resolve, 500)); // Thinking delay
          const aiMove = await AILogicService.getAiMove(newState, difficulty);
          
          // Issue 7: Guard against invalid AI moves (full board edge case)
          if (aiMove[0] === -1 && aiMove[1] === -1) {
            console.warn('AI returned no valid move (board may be full)');
            moveLockRef.current = false;
            return;
          }
          
          const afterAiMove = GameLogicService.makeMove(newState, aiMove[0], aiMove[1]);

          setGameState(afterAiMove);
          setLastMove({ row: aiMove[0], col: aiMove[1] });

          // Check if game ended after AI move
          if (afterAiMove.gameStatus !== GameStatus.Playing) {
            await saveStats(afterAiMove);
          }
        } finally {
          setIsAiThinking(false);
          moveLockRef.current = false; // Issue 3: Release lock after AI move
        }
      } else {
        moveLockRef.current = false; // Release lock if no AI turn
      }
    },
    [gameState, isAiThinking, difficulty, saveStats]
  );

  // Return to menu
  const returnToMenu = useCallback(() => {
    setGameState(null);
    setLastMove(null);
    setGameMode(GameMode.Menu);
    setSelectedMode('');
  }, []);

  // Get game status message
  const getGameStatus = (): string => {
    if (!gameState) return '';

    if (gameState.gameStatus === GameStatus.Won) {
      const winner = gameState.players.find((p) => p.symbol === gameState.winner);
      return `${winner?.type === PlayerType.Human ? 'You Win!' : 'AI Wins!'}`;
    }

    if (gameState.gameStatus === GameStatus.Draw) {
      return "It's a Draw!";
    }

    if (isAiThinking) {
      return 'AI is thinking...';
    }

    return `${gameState.currentPlayer === PlayerType.X ? 'Your' : "AI's"} Turn`;
  };

  // Get status class for styling
  const getStatusClass = (): string => {
    if (gameState?.gameStatus === GameStatus.Won) {
      return gameState.winner === PlayerType.X ? 'player-wins' : 'ai-wins';
    }
    return '';
  };

  // Render menu
  if (gameMode === GameMode.Menu) {
    return (
      <div className="app">
        <h1 className="game-title">POTICTAC</h1>

        <div className="menu">
          <div className="player-name-section">
            <label className="section-label">Your Name</label>
            <div className="player-name-input">
              <input
                type="text"
                placeholder="Enter Your Name"
                value={playerName}
                onChange={(e) => {
                  setPlayerName(e.target.value);
                  setNameWarning(false); // Clear warning on edit
                }}
                className={`name-input ${playerName ? 'has-value' : ''} ${nameWarning ? 'warning' : ''}`}
              />
              {playerName && <span className="input-check">✓</span>}
            </div>
            {nameWarning && (
              <div className="name-warning">⚠️ Stats won't be saved without a name!</div>
            )}
          </div>

          <div className="mode-selection">
            <label className="section-label">Game Mode</label>
            <div className="mode-buttons">
              <button
                className={`mode-button primary ${selectedMode === 'single' ? 'selected' : ''}`}
                onClick={() => setSelectedMode('single')}
                disabled={isStartingGame}
              >
                <span className="mode-icon">🤖</span>
                <span className="mode-text">Single Player</span>
              </button>
            </div>
          </div>

          {selectedMode === 'single' && (
            <div className="difficulty-section animate-in">
              <DifficultySelector
                currentDifficulty={difficulty}
                onDifficultyChange={setDifficulty}
              />
            </div>
          )}

          {canStartGame && (
            <div className="start-game-section">
              <button
                className="start-game-button"
                onClick={handleStartGame}
                disabled={isStartingGame}
              >
                {isStartingGame ? (
                  <>
                    <span className="loading-spinner"></span>
                    <span>Starting...</span>
                  </>
                ) : (
                  <>
                    <span className="start-icon">▶</span>
                    <span>Start Game</span>
                  </>
                )}
              </button>
            </div>
          )}

          <div className="stats-links">
            <button className="stats-link" onClick={() => navigate('/leaderboard')}>
              🏆 Leaderboard
            </button>
          </div>
        </div>
      </div>
    );
  }

  // Render game
  if (gameState) {
    return (
      <div className="app">
        <div className="game-container">
          {gameState.gameStatus === GameStatus.Won && (
            <div className={`game-status-overlay ${getStatusClass()}`}>
              {getGameStatus()}
            </div>
          )}

          <div className="game-status">{getGameStatus()}</div>

          <GameBoard
            gameState={gameState}
            onCellClick={handleCellClick}
            lastMove={lastMove}
          />

          <div className="game-controls">
            <button 
              onClick={returnToMenu} 
              className="menu-button"
              disabled={isAiThinking} // Issue 10: Prevent navigation during AI move
            >
              {isAiThinking ? 'AI Thinking...' : 'Back to Menu'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return null;
}
