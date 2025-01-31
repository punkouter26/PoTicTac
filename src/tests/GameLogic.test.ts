import '@jest/globals';
import { createInitialState, makeMove, undoMove, redoMove } from '../utils/gameLogic';
import { GameState, Player } from '../types/GameTypes';
import { createInitialStats } from '../utils/statsManager';

const createTestPlayers = (): [Player, Player] => [
    { id: '1', name: 'Player 1', type: 'human', symbol: 1, stats: createInitialStats() },
    { id: '2', name: 'Player 2', type: 'human', symbol: 2, stats: createInitialStats() }
];

// Add comprehensive test suite
describe('Game Logic Tests', () => {
    test('Should correctly identify winning combinations', () => {
        // Add test cases
    });
    
    test('Should properly handle diagonal wins', () => {
        // Add test cases
    });

    describe('Undo/Redo Tests', () => {
        let initialState: GameState;

        beforeEach(() => {
            initialState = createInitialState(1, createTestPlayers());
        });

        test('Should correctly undo a move', () => {
            // Make a move
            const stateAfterMove = makeMove(initialState, 0, 0);
            expect(stateAfterMove.board[0][0]).toBe(1);
            expect(stateAfterMove.currentPlayer).toBe(2);

            // Undo the move
            const stateAfterUndo = undoMove(stateAfterMove);
            expect(stateAfterUndo.board[0][0]).toBe(0);
            expect(stateAfterUndo.currentPlayer).toBe(1);
            expect(stateAfterUndo.moveHistory).toHaveLength(0);
            expect(stateAfterUndo.undoStack).toHaveLength(1);
        });

        test('Should correctly redo a move', () => {
            // Make and undo a move
            const stateAfterMove = makeMove(initialState, 0, 0);
            const stateAfterUndo = undoMove(stateAfterMove);

            // Redo the move
            const stateAfterRedo = redoMove(stateAfterUndo);
            expect(stateAfterRedo.board[0][0]).toBe(1);
            expect(stateAfterRedo.currentPlayer).toBe(2);
            expect(stateAfterRedo.moveHistory).toHaveLength(1);
            expect(stateAfterRedo.undoStack).toHaveLength(0);
        });

        test('Should not undo when no moves are available', () => {
            const stateAfterUndo = undoMove(initialState);
            expect(stateAfterUndo).toEqual(initialState);
        });

        test('Should not redo when no undone moves are available', () => {
            const stateAfterRedo = redoMove(initialState);
            expect(stateAfterRedo).toEqual(initialState);
        });

        test('Should clear redo stack after new move', () => {
            // Make and undo a move
            const stateAfterMove = makeMove(initialState, 0, 0);
            const stateAfterUndo = undoMove(stateAfterMove);
            expect(stateAfterUndo.undoStack).toHaveLength(1);

            // Make a different move
            const stateAfterNewMove = makeMove(stateAfterUndo, 1, 1);
            expect(stateAfterNewMove.undoStack).toHaveLength(0);
            expect(stateAfterNewMove.redoStack).toHaveLength(0);
        });
    });
}); 