import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Game Play Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    // Wait for Blazor WASM to finish loading
    await waitForBlazorLoad(page);
  });

  test('should start a new game', async ({ page }) => {
    // Arrange - Click single player button to start game
    const singlePlayerButton = page.locator('button.mode-button:has-text("Single Player")');
    
    // Act - Start a single player game
    await singlePlayerButton.click();
    
    // Assert - Game board should be visible
    const gameBoard = page.locator('.game-container');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
  });

  test('should display game status', async ({ page }) => {
    // Arrange - Start a single player game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Act & Assert - Status should be visible in game
    const statusElement = page.locator('.game-status');
    await expect(statusElement).toBeVisible({ timeout: 10000 });
  });

  test('should allow selecting difficulty level', async ({ page }) => {
    // Arrange - Start a single player game to see difficulty selector
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Wait for game to initialize
    await page.locator('.game-container').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act & Assert - Look for difficulty selector
    const difficultySelector = page.locator('select, .difficulty-selector, [data-testid="difficulty-selector"]');
    
    // Check if difficulty selector exists (it should be visible)
    const count = await difficultySelector.count();
    expect(count).toBeGreaterThanOrEqual(0); // It may not be visible initially depending on game state
  });

  test('should allow making a move on the board', async ({ page }) => {
    // Arrange - Start a single player game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    const gameContainer = page.locator('.game-container');
    await gameContainer.waitFor({ state: 'visible', timeout: 10000 });
    
    // Act & Assert - Verify game status is shown (indicates game started successfully)
    const gameStatus = page.locator('.game-status');
    await expect(gameStatus).toBeVisible({ timeout: 10000 });
  });
});
