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

    // Assert - Check for title (flexible selector)
    const gameTitle = page.locator('.game-title, h1:has-text("POTICTAC")').first();
    await expect(gameTitle).toBeVisible({ timeout: 15000 });
    await expect(gameTitle).toContainText('POTICTAC');
  });

  test('should display game mode buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for mode selection buttons with flexible selectors
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    const multiplayerButton = page.locator('button.mode-button').filter({ hasText: 'Multiplayer' });
    
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await expect(multiplayerButton).toBeVisible({ timeout: 15000 });
  });

  test('should have player name input', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for name input (flexible selector)
    const nameInput = page.locator('input.name-input, input[placeholder*="Name"]').first();
    await expect(nameInput).toBeVisible({ timeout: 15000 });
  });

  test('should have stats navigation buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for stats buttons (Player Stats and Leaderboard)
    const playerStatsButton = page.locator('button.stats-button').filter({ hasText: 'Player Stats' });
    const leaderboardButton = page.locator('button.stats-button').filter({ hasText: 'Leaderboard' });
    await expect(playerStatsButton).toBeVisible({ timeout: 15000 });
    await expect(leaderboardButton).toBeVisible({ timeout: 15000 });
  });
});