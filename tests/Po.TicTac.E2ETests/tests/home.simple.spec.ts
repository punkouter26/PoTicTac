import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { waitForBlazorLoad } from './helpers';

/**
 * Simplified E2E tests for PoTicTac Home Page
 * These tests work with the current Blazor UI structure
 */
test.describe('PoTicTac Home Page - Simple Tests', () => {

  test('should load home page and display title', async ({ page }) => {
    // Arrange & Act - Navigate to homepage and wait for Blazor to load
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);

    // Assert - Check for title (Blazor WASM may take time to load)
    await expect(page.locator('h1.game-title')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('h1.game-title')).toHaveText('POTICTAC');
  });  test('should display game mode buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for mode selection buttons with updated selectors (wait for Blazor)
    const singlePlayerButton = page.locator('button.mode-button:has-text("Single Player")');
    const multiplayerButton = page.locator('button.mode-button:has-text("Multiplayer")');
    
    await expect(singlePlayerButton).toBeVisible({ timeout: 10000 });
    await expect(multiplayerButton).toBeVisible();
  });

  test('should have player name input', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for name input with updated placeholder text (wait for Blazor)
    const nameInput = page.locator('input.name-input[placeholder="Enter Your Name"]');
    await expect(nameInput).toBeVisible({ timeout: 10000 });
  });

  test('should have stats navigation buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for stats buttons (Player Stats and Leaderboard)
    const playerStatsButton = page.locator('button.stats-button:has-text("Player Stats")');
    const leaderboardButton = page.locator('button.stats-button:has-text("Leaderboard")');
    await expect(playerStatsButton).toBeVisible({ timeout: 10000 });
    await expect(leaderboardButton).toBeVisible({ timeout: 10000 });
  });
});