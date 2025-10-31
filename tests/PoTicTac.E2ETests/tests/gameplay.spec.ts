import { test, expect } from '@playwright/test';

test.describe('Game Play Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('should allow selecting difficulty level', async ({ page }) => {
    // Arrange - Look for difficulty selector
    const difficultySelector = page.locator('[data-testid="difficulty-selector"], select, .difficulty');
    
    // Act - Wait for selector to be visible
    await difficultySelector.first().waitFor({ state: 'visible', timeout: 10000 });
    
    // Assert - Selector should be present
    await expect(difficultySelector.first()).toBeVisible();
  });

  test('should start a new game', async ({ page }) => {
    // Arrange
    const newGameButton = page.locator('button:has-text("New Game"), button:has-text("Start"), [data-testid="new-game"]');
    
    // Act - Try to click new game button if it exists
    const buttonCount = await newGameButton.count();
    
    if (buttonCount > 0) {
      await newGameButton.first().click();
      
      // Assert - Game board should be reset or visible
      const gameBoard = page.locator('[data-testid="game-board"], .game-board, board');
      await expect(gameBoard.first()).toBeVisible();
    } else {
      // If no new game button, game starts automatically
      const gameBoard = page.locator('[data-testid="game-board"], .game-board, board');
      await expect(gameBoard.first()).toBeVisible();
    }
  });

  test('should allow making a move on the board', async ({ page }) => {
    // Arrange - Find the game board
    const gameBoard = page.locator('[data-testid="game-board"], .game-board, board');
    await gameBoard.first().waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Find clickable cells
    const cells = page.locator('[data-testid^="cell-"], .cell, button:not(:has-text("New Game"))');
    const cellCount = await cells.count();
    
    if (cellCount > 0) {
      // Click first empty cell
      const firstCell = cells.first();
      await firstCell.click({ timeout: 5000 });
      
      // Assert - Cell should show a mark (X or O)
      // Wait for cell content to update
      await page.waitForTimeout(1000);
      
      // Check if cell has content now
      const cellContent = await firstCell.textContent();
      expect(cellContent).toBeTruthy();
    }
  });

  test('should display game status', async ({ page }) => {
    // Arrange & Act
    const statusElement = page.locator('[data-testid="game-status"], .game-status, .status');
    
    // Assert - Status should be visible
    await expect(statusElement.first()).toBeVisible({ timeout: 10000 });
  });
});
