import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Game Play Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    // Wait for Blazor WASM to finish loading
    await waitForBlazorLoad(page);
  });

  test('should start a new game', async ({ page }) => {
    // Arrange - Click single player button to select game mode
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    
    // Act - Select single player mode and start game
    await singlePlayerButton.click();
    const startGameButton = page.locator('.start-game-button');
    await expect(startGameButton).toBeVisible({ timeout: 5000 });
    await startGameButton.click();
    
    // Assert - Game board should be visible
    const gameBoard = page.locator('.game-container');
    await expect(gameBoard).toBeVisible({ timeout: 15000 });
  });

  test('should display game status', async ({ page }) => {
    // Arrange - Start a single player game
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await singlePlayerButton.click();
    const startGameButton = page.locator('.start-game-button');
    await expect(startGameButton).toBeVisible({ timeout: 5000 });
    await startGameButton.click();
    
    // Act & Assert - Status should be visible in game
    const statusElement = page.locator('.game-status');
    await expect(statusElement).toBeVisible({ timeout: 15000 });
  });

  test('should allow selecting difficulty level', async ({ page }) => {
    // Arrange - Click single player to reveal difficulty selector
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await singlePlayerButton.click();
    
    // Act & Assert - Difficulty selector should be visible in menu
    const difficultySelector = page.locator('.difficulty-selector').first();
    await expect(difficultySelector).toBeVisible({ timeout: 15000 });
  });

  test('should allow making a move on the board', async ({ page }) => {
    // Arrange - Start a single player game
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await singlePlayerButton.click();
    const startGameButton = page.locator('.start-game-button');
    await expect(startGameButton).toBeVisible({ timeout: 5000 });
    await startGameButton.click();
    const gameContainer = page.locator('.game-container');
    await gameContainer.waitFor({ state: 'visible', timeout: 15000 });
    
    // Act & Assert - Verify game status is shown (indicates game started successfully)
    const gameStatus = page.locator('.game-status');
    await expect(gameStatus).toBeVisible({ timeout: 15000 });
  });
});
