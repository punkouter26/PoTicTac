const express = require('express');
const http = require('http');
const { Server } = require("socket.io");
const path = require('path');

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: "*", // Allow all origins for simplicity in development
    methods: ["GET", "POST"]
  }
});

const PORT = process.env.PORT || 3001; // Use a different port than the frontend

// --- Game State Management (In-Memory) ---
const games = {}; // Store active games { gameId: { players: [socketId1, socketId2], gameState: {...}, playerNames: {socketId: name} } }
const playerSockets = {}; // Map socket.id to gameId { socketId: gameId }

// Helper function to generate simple game IDs (replace with more robust method if needed)
const generateGameId = () => {
  return Math.random().toString(36).substring(2, 8).toUpperCase();
};

// Placeholder for actual game logic import (assuming it exists or will be created)
// For now, we'll use a simplified state
const createInitialGameState = (player1Name, player2Name) => {
  // This needs to align with the GameState type in the frontend
  return {
    board: Array(6).fill(null).map(() => Array(7).fill(0)), // Example 6x7 board
    players: [
      { id: 'socketId1', name: player1Name, symbol: 1 }, // Placeholder IDs
      { id: 'socketId2', name: player2Name, symbol: 2 }
    ],
    currentPlayer: 1, // Player 1 starts
    gameStatus: 'playing', // playing, won, draw
    winner: null,
    gameId: null // Will be set when game is created
  };
};

// Placeholder for move logic
const applyMove = (gameState, move, playerSymbol) => {
  // Basic validation and update - replace with actual game logic
  // --- Standard Tic-Tac-Toe Logic ---
  if (
    gameState.gameStatus !== 'playing' ||      // Game must be active
    gameState.currentPlayer !== playerSymbol || // Must be the player's turn
    move.row < 0 || move.row >= gameState.board.length || // Row must be in bounds
    move.col < 0 || move.col >= gameState.board[0].length || // Col must be in bounds
    gameState.board[move.row][move.col] !== 0 // Cell must be empty
  ) {
    console.log(`Invalid move: row=${move.row}, col=${move.col}, player=${playerSymbol}, boardVal=${gameState.board[move.row]?.[move.col]}`);
    return gameState; // Invalid move (out of bounds, cell taken, or not player's turn)
  }

  // Place piece directly at the clicked cell
  const newBoard = gameState.board.map(r => [...r]);
  newBoard[move.row][move.col] = playerSymbol;

  // TODO: Check for win/draw condition here (using newBoard)
  const gameStatus = 'playing'; // Placeholder - Implement win/draw check
  const winner = null; // Placeholder

  // Return the updated state AND the actual move coordinates
  return {
    newState: {
        ...gameState,
        board: newBoard,
        currentPlayer: playerSymbol === 1 ? 2 : 1, // Switch player
        gameStatus: gameStatus, // Update based on win/draw check
        winner: winner // Update based on win/draw check
    },
    // Return the coordinates where the piece was actually placed (which is the clicked cell now)
    placedMove: { row: move.row, col: move.col } 
  };
};


// Serve static files from the React build directory (assuming it's ../build)
// Adjust the path if your build output is elsewhere
const buildPath = path.join(__dirname, '..', 'build');
app.use(express.static(buildPath));

io.on('connection', (socket) => {
  console.log('User connected:', socket.id);

  socket.on('create_game', ({ playerName }) => {
    console.log(`Create game request from ${playerName} (${socket.id})`);
    const gameId = generateGameId();
    games[gameId] = {
      players: [socket.id],
      gameState: null, // Game state created when second player joins
      playerNames: { [socket.id]: playerName }
    };
    playerSockets[socket.id] = gameId;
    socket.join(gameId);
    console.log(`Game ${gameId} created by ${playerName}. Waiting for opponent.`);
    socket.emit('game_created', { gameId });
  });

  socket.on('join_game', ({ gameCode, playerName }) => {
    console.log(`Join game request for ${gameCode} from ${playerName} (${socket.id})`);
    const game = games[gameCode];

    if (!game) {
      console.log(`Game ${gameCode} not found.`);
      socket.emit('lobby_error', { message: `Game code "${gameCode}" not found.` });
      return;
    }

    if (game.players.length >= 2) {
      console.log(`Game ${gameCode} is full.`);
      socket.emit('lobby_error', { message: `Game "${gameCode}" is already full.` });
      return;
    }

    if (game.players.includes(socket.id)) {
      console.log(`Player ${socket.id} already in game ${gameCode}.`);
      // Maybe rejoin logic? For now, just ignore or send error.
      socket.emit('lobby_error', { message: `You are already in this game.` });
      return;
    }

    // Add player 2
    game.players.push(socket.id);
    game.playerNames[socket.id] = playerName;
    playerSockets[socket.id] = gameCode;
    socket.join(gameCode);

    console.log(`${playerName} (${socket.id}) joined game ${gameCode}. Starting game.`);

    // Create initial game state
    const player1SocketId = game.players[0];
    const player2SocketId = game.players[1];
    const player1Name = game.playerNames[player1SocketId];
    const player2Name = game.playerNames[player2SocketId];
    game.gameState = createInitialGameState(player1Name, player2Name);
    game.gameState.gameId = gameCode; // Add gameId to state
    // Assign socket IDs to players in gameState
    game.gameState.players[0].id = player1SocketId;
    game.gameState.players[1].id = player2SocketId;


    // Notify both players that the game has started
    io.to(gameCode).emit('game_joined', {
      gameId: gameCode,
      isHost: false, // This might need adjustment based on who is considered host
      initialState: game.gameState
    });
     // Send specific isHost=true to creator?
     io.to(player1SocketId).emit('game_joined', { gameId: gameCode, isHost: true, initialState: game.gameState });
     io.to(player2SocketId).emit('game_joined', { gameId: gameCode, isHost: false, initialState: game.gameState });

  });

  socket.on('make_move', ({ gameId, move }) => {
    const game = games[gameId];
    if (!game || !game.gameState || game.gameState.gameStatus !== 'playing') {
      console.log(`Invalid move request for game ${gameId} from ${socket.id}`);
      // Maybe send an error back?
      return;
    }

    // Determine player symbol based on socket ID
    const playerIndex = game.players.indexOf(socket.id);
    if (playerIndex === -1) {
        console.log(`Player ${socket.id} not found in game ${gameId}`);
        return; // Player not part of this game
    }
    const playerSymbol = game.gameState.players[playerIndex].symbol;

    console.log(`Move received for game ${gameId} from ${socket.id} (Player ${playerSymbol}):`, move);

    // Apply the move using game logic
    const updatedGameState = applyMove(game.gameState, move, playerSymbol);

    if (updatedGameState === game.gameState) {
      console.log(`Invalid move in game ${gameId} by Player ${playerSymbol}. State unchanged.`);
      // Optionally notify the player of invalid move
      socket.emit('invalid_move', { message: "Invalid move." });
      return;
    }

    // Apply the move using game logic
    const moveResult = applyMove(game.gameState, move, playerSymbol);

    if (moveResult.newState === game.gameState) { // Check if state changed
      console.log(`Invalid move in game ${gameId} by Player ${playerSymbol}. State unchanged.`);
      // Optionally notify the player of invalid move
      socket.emit('invalid_move', { message: "Invalid move." });
      return;
    }

    game.gameState = moveResult.newState; // Update server state

    // Broadcast the updated state AND the placed move to all players in the room
    io.to(gameId).emit('game_state_update', {
        gameState: game.gameState,
        placedMove: moveResult.placedMove // Include where the piece landed
    });

    // Check if game ended
    if (game.gameState.gameStatus === 'won' || game.gameState.gameStatus === 'draw') {
      console.log(`Game ${gameId} ended. Status: ${game.gameState.gameStatus}, Winner: ${game.gameState.winner}`);
      io.to(gameId).emit('game_over', { winner: game.gameState.winner, gameState: game.gameState });
      // Consider cleaning up the game from memory after a delay?
    }
  });

  socket.on('request_rematch', ({ gameId }) => {
    const game = games[gameId];
    if (!game) return;
    console.log(`Rematch requested for game ${gameId} by ${socket.id}`);
    // Notify the *other* player about the rematch request
    const otherPlayerSocketId = game.players.find(id => id !== socket.id);
    if (otherPlayerSocketId) {
      io.to(otherPlayerSocketId).emit('rematch_requested', { requesterId: socket.id });
    }
  });

  socket.on('accept_rematch', ({ gameId }) => {
     const game = games[gameId];
     if (!game || game.players.length < 2) return;
     console.log(`Rematch accepted for game ${gameId} by ${socket.id}`);
     // Reset game state
     const player1Name = game.playerNames[game.players[0]];
     const player2Name = game.playerNames[game.players[1]];
     game.gameState = createInitialGameState(player1Name, player2Name);
     game.gameState.gameId = gameId;
     game.gameState.players[0].id = game.players[0];
     game.gameState.players[1].id = game.players[1];

     // Notify both players
     io.to(gameId).emit('game_started', { gameState: game.gameState }); // Or use game_state_update?
     io.to(gameId).emit('game_state_update', game.gameState);
  });


  socket.on('leave_game', ({ gameId }) => {
    console.log(`Player ${socket.id} leaving game ${gameId}`);
    socket.leave(gameId);
    const game = games[gameId];
    if (game) {
      // Notify other player if they exist
      const otherPlayerSocketId = game.players.find(id => id !== socket.id);
      if (otherPlayerSocketId) {
        io.to(otherPlayerSocketId).emit('opponent_left', { message: `${game.playerNames[socket.id]} left the game.` });
        // Optionally end the game or allow waiting for a new opponent
      }
      // Remove player from game (or handle cleanup differently)
      game.players = game.players.filter(id => id !== socket.id);
      delete game.playerNames[socket.id];
      if (game.players.length === 0) {
        console.log(`Game ${gameId} is empty, deleting.`);
        delete games[gameId];
      }
    }
    delete playerSockets[socket.id];
  });


  socket.on('disconnect', () => {
    console.log('User disconnected:', socket.id);
    const gameId = playerSockets[socket.id];
    if (gameId && games[gameId]) {
      console.log(`Player ${socket.id} disconnected from game ${gameId}`);
      const game = games[gameId];
      // Notify other player
      const otherPlayerSocketId = game.players.find(id => id !== socket.id);
       if (otherPlayerSocketId) {
         io.to(otherPlayerSocketId).emit('opponent_disconnected', { message: `${game.playerNames[socket.id]} disconnected.` });
         // Optionally end the game
         // game.gameState.gameStatus = 'aborted'; // Example
         // io.to(otherPlayerSocketId).emit('game_state_update', game.gameState);
       }

      // Clean up player and potentially game
      game.players = game.players.filter(id => id !== socket.id);
      delete game.playerNames[socket.id];
      if (game.players.length === 0) {
        console.log(`Game ${gameId} is empty after disconnect, deleting.`);
        delete games[gameId];
      }
    }
    delete playerSockets[socket.id]; // Remove mapping
  });
});

// Serve the React app for any other requests (client-side routing)
app.get('*', (req, res) => {
  res.sendFile(path.join(buildPath, 'index.html'));
});


server.listen(PORT, () => {
  console.log(`Server listening on *:${PORT}`);
});
