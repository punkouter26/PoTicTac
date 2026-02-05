import { test, expect } from '@playwright/test';
import { waitForReactLoad } from './helpers';

test.describe('Quick Game and Leaderboard Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForReactLoad(page);
  });

  test('should play a quick game on easy single player and view leaderboard', async ({ page }) => {
    // Step 1: Enter player name
    const nameInput = page.locator('input[placeholder="Enter Your Name"]');
    await expect(nameInput).toBeVisible({ timeout: 10000 });
    await nameInput.fill('E2ETestPlayer');

    // Step 2: Select Single Player mode
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 10000 });
    await singlePlayerButton.click();

    // Step 3: Select Easy difficulty
    const easyButton = page.locator('button.difficulty-button').filter({ hasText: 'Easy' });
    await expect(easyButton).toBeVisible({ timeout: 10000 });
    await easyButton.click();

    // Step 4: Start the game
    const startButton = page.locator('button.start-game-button');
    await expect(startButton).toBeVisible({ timeout: 10000 });
    await startButton.click();

    // Step 5: Verify game board is visible
    const gameContainer = page.locator('.game-container');
    await expect(gameContainer).toBeVisible({ timeout: 15000 });

    // Step 6: Make moves until game ends
    // Play moves on the board - click available cells
    let gameEnded = false;
    let moveCount = 0;
    const maxMoves = 36; // 6x6 board

    while (!gameEnded && moveCount < maxMoves) {
      // Check if game ended
      const statusOverlay = page.locator('.game-status-overlay');
      const statusText = await page.locator('.game-status').textContent();
      
      if (statusText?.includes('Win') || statusText?.includes('Draw')) {
        gameEnded = true;
        break;
      }

      // Wait for our turn (not AI thinking)
      if (statusText?.includes('AI is thinking')) {
        await page.waitForTimeout(600);
        continue;
      }

      // Try to click an empty cell
      const cells = page.locator('.cell:not(.x):not(.o)');
      const count = await cells.count();
      
      if (count > 0) {
        await cells.first().click();
        moveCount++;
        // Wait for move to process and possible AI response
        await page.waitForTimeout(800);
      } else {
        gameEnded = true;
      }
    }

    // Step 7: Verify game ended
    const gameStatus = page.locator('.game-status');
    await expect(gameStatus).toBeVisible();
    const finalStatus = await gameStatus.textContent();
    expect(finalStatus).toMatch(/Win|Draw/);

    // Step 8: Return to menu
    const backButton = page.locator('button.menu-button').filter({ hasText: 'Back to Menu' });
    await expect(backButton).toBeVisible({ timeout: 5000 });
    await backButton.click();

    // Step 9: Navigate to Leaderboard
    const leaderboardLink = page.locator('button.stats-link').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardLink).toBeVisible({ timeout: 10000 });
    await leaderboardLink.click();

    // Step 10: Verify leaderboard page
    const leaderboardTitle = page.locator('h1').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardTitle).toBeVisible({ timeout: 10000 });

    // Verify leaderboard table OR empty message is visible (API might be offline)
    const tableOrMessage = page.locator('.leaderboard-table, .empty-message, .error-message');
    await expect(tableOrMessage).toBeVisible({ timeout: 10000 });

    // Step 11: Navigate back to game
    const backToGameButton = page.locator('button.back-button').filter({ hasText: 'Back to Game' });
    await expect(backToGameButton).toBeVisible({ timeout: 5000 });
    await backToGameButton.click();

    // Verify we're back at menu
    await expect(singlePlayerButton).toBeVisible({ timeout: 10000 });
  });

  test('should be able to view leaderboard directly', async ({ page }) => {
    // Navigate to leaderboard from menu
    const leaderboardLink = page.locator('button.stats-link').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardLink).toBeVisible({ timeout: 10000 });
    await leaderboardLink.click();

    // Verify leaderboard page loads
    const leaderboardTitle = page.locator('h1').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardTitle).toBeVisible({ timeout: 10000 });

    // Verify structure elements are present
    await expect(page.locator('.leaderboard-page')).toBeVisible();
  });
});
