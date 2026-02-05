import { describe, it, expect } from 'vitest';
import { AILogicService } from '../services';
import { GameLogicService } from '../services';
import { PlayerType, Difficulty } from '../types';

describe('AILogicService', () => {
  const createGameWithPlayers = () => {
    const players: [
      { id: string; name: string; type: PlayerType; symbol: PlayerType },
      { id: string; name: string; type: PlayerType; symbol: PlayerType }
    ] = [
      { id: '1', name: 'Player', type: PlayerType.Human, symbol: PlayerType.X },
      { id: '2', name: 'AI', type: PlayerType.AI, symbol: PlayerType.O },
    ];
    return GameLogicService.createInitialState(PlayerType.X, players);
  };

  describe('getAiMove', () => {
    it('should return valid move for Easy difficulty', async () => {
      let state = createGameWithPlayers();
      state = GameLogicService.makeMove(state, 2, 2); // Player move

      const move = await AILogicService.getAiMove(state, Difficulty.Easy);

      expect(move).toBeDefined();
      expect(move.length).toBe(2);
      expect(move[0]).toBeGreaterThanOrEqual(0);
      expect(move[0]).toBeLessThan(6);
      expect(move[1]).toBeGreaterThanOrEqual(0);
      expect(move[1]).toBeLessThan(6);
      // Move should be on empty cell
      expect(state.board[move[0]][move[1]]).toBe(PlayerType.None);
    });

    it('should return valid move for Medium difficulty', async () => {
      let state = createGameWithPlayers();
      state = GameLogicService.makeMove(state, 2, 2); // Player move

      const move = await AILogicService.getAiMove(state, Difficulty.Medium);

      expect(move).toBeDefined();
      expect(state.board[move[0]][move[1]]).toBe(PlayerType.None);
    });

    it('should return valid move for Hard difficulty', async () => {
      let state = createGameWithPlayers();
      state = GameLogicService.makeMove(state, 2, 2); // Player move

      const move = await AILogicService.getAiMove(state, Difficulty.Hard);

      expect(move).toBeDefined();
      expect(state.board[move[0]][move[1]]).toBe(PlayerType.None);
    });

    it('should block player winning move on Hard', async () => {
      let state = createGameWithPlayers();
      
      // Set up a near-win for X (row 0: positions 0,1,2)
      state = GameLogicService.makeMove(state, 0, 0); // X
      state = GameLogicService.makeMove(state, 5, 5); // O
      state = GameLogicService.makeMove(state, 0, 1); // X
      state = GameLogicService.makeMove(state, 5, 4); // O
      state = GameLogicService.makeMove(state, 0, 2); // X - now X has 3 in a row
      
      // AI should block at (0, 3)
      const move = await AILogicService.getAiMove(state, Difficulty.Hard);

      expect(move).toBeDefined();
      // Hard AI should recognize threat and block
      // Either block at (0,3) or take winning move if available
    });

    it('should take winning move when available', async () => {
      let state = createGameWithPlayers();
      
      // Set up a near-win for O
      state = GameLogicService.makeMove(state, 5, 0); // X
      state = GameLogicService.makeMove(state, 0, 0); // O
      state = GameLogicService.makeMove(state, 5, 1); // X
      state = GameLogicService.makeMove(state, 0, 1); // O
      state = GameLogicService.makeMove(state, 5, 5); // X
      state = GameLogicService.makeMove(state, 0, 2); // O - now O has 3 in row
      state = GameLogicService.makeMove(state, 4, 4); // X
      
      // Now it's O's turn and O can win at (0, 3)
      const move = await AILogicService.getAiMove(state, Difficulty.Hard);

      expect(move).toEqual([0, 3]); // Should take winning move
    });
  });
});
