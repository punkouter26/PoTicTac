import { describe, it, expect } from 'vitest';
import {
  GameLogicService,
} from '../services';
import {
  PlayerType,
  GameStatus,
  BOARD_SIZE,
} from '../types';

describe('GameLogicService', () => {
  describe('createInitialState', () => {
    it('should create initial state with empty board', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      const state = GameLogicService.createInitialState(PlayerType.X, players);

      expect(state.currentPlayer).toBe(PlayerType.X);
      expect(state.gameStatus).toBe(GameStatus.Playing);
      expect(state.winner).toBeNull();
      expect(state.board.length).toBe(BOARD_SIZE);
      expect(state.board[0].length).toBe(BOARD_SIZE);
      
      // All cells should be empty
      for (let row = 0; row < BOARD_SIZE; row++) {
        for (let col = 0; col < BOARD_SIZE; col++) {
          expect(state.board[row][col]).toBe(PlayerType.None);
        }
      }
    });
  });

  describe('makeMove', () => {
    it('should place a piece on empty cell', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      const state = GameLogicService.createInitialState(PlayerType.X, players);
      const newState = GameLogicService.makeMove(state, 0, 0);

      expect(newState.board[0][0]).toBe(PlayerType.X);
      expect(newState.currentPlayer).toBe(PlayerType.O);
      expect(newState.moveHistory.length).toBe(1);
    });

    it('should not allow move on occupied cell', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      let state = GameLogicService.createInitialState(PlayerType.X, players);
      state = GameLogicService.makeMove(state, 0, 0);
      const sameState = GameLogicService.makeMove(state, 0, 0);

      expect(sameState).toBe(state); // Same reference means no change
    });

    it('should detect horizontal win', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      let state = GameLogicService.createInitialState(PlayerType.X, players);
      
      // X plays row 0: 0,1,2,3 (with O playing row 1)
      state = GameLogicService.makeMove(state, 0, 0); // X
      state = GameLogicService.makeMove(state, 1, 0); // O
      state = GameLogicService.makeMove(state, 0, 1); // X
      state = GameLogicService.makeMove(state, 1, 1); // O
      state = GameLogicService.makeMove(state, 0, 2); // X
      state = GameLogicService.makeMove(state, 1, 2); // O
      state = GameLogicService.makeMove(state, 0, 3); // X wins!

      expect(state.gameStatus).toBe(GameStatus.Won);
      expect(state.winner).toBe(PlayerType.X);
      expect(state.winningCells).toHaveLength(4);
    });

    it('should detect vertical win', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      let state = GameLogicService.createInitialState(PlayerType.X, players);
      
      // X plays column 0: rows 0,1,2,3 (with O playing column 1)
      state = GameLogicService.makeMove(state, 0, 0); // X
      state = GameLogicService.makeMove(state, 0, 1); // O
      state = GameLogicService.makeMove(state, 1, 0); // X
      state = GameLogicService.makeMove(state, 1, 1); // O
      state = GameLogicService.makeMove(state, 2, 0); // X
      state = GameLogicService.makeMove(state, 2, 1); // O
      state = GameLogicService.makeMove(state, 3, 0); // X wins!

      expect(state.gameStatus).toBe(GameStatus.Won);
      expect(state.winner).toBe(PlayerType.X);
    });

    it('should detect diagonal win', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      let state = GameLogicService.createInitialState(PlayerType.X, players);
      
      // X plays diagonal: (0,0), (1,1), (2,2), (3,3)
      state = GameLogicService.makeMove(state, 0, 0); // X
      state = GameLogicService.makeMove(state, 0, 5); // O
      state = GameLogicService.makeMove(state, 1, 1); // X
      state = GameLogicService.makeMove(state, 1, 5); // O
      state = GameLogicService.makeMove(state, 2, 2); // X
      state = GameLogicService.makeMove(state, 2, 5); // O
      state = GameLogicService.makeMove(state, 3, 3); // X wins!

      expect(state.gameStatus).toBe(GameStatus.Won);
      expect(state.winner).toBe(PlayerType.X);
    });
  });

  describe('getAvailableMoves', () => {
    it('should return all cells for empty board', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      const state = GameLogicService.createInitialState(PlayerType.X, players);
      const moves = GameLogicService.getAvailableMoves(state.board);

      expect(moves.length).toBe(BOARD_SIZE * BOARD_SIZE);
    });

    it('should return fewer moves after pieces placed', () => {
      const players: [
        { id: string; name: string; type: PlayerType; symbol: PlayerType },
        { id: string; name: string; type: PlayerType; symbol: PlayerType }
      ] = [
        { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
        { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
      ];

      let state = GameLogicService.createInitialState(PlayerType.X, players);
      state = GameLogicService.makeMove(state, 0, 0);
      state = GameLogicService.makeMove(state, 1, 1);

      const moves = GameLogicService.getAvailableMoves(state.board);
      expect(moves.length).toBe(BOARD_SIZE * BOARD_SIZE - 2);
    });
  });
});
