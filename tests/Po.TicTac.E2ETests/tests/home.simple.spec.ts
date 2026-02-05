import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { waitForBlazorLoad } from './helpers';

/**
 * Consolidated E2E tests for PoTicTac Home Page
 * Covers core functionality, accessibility, and responsive design
 */
test.describe('PoTicTac Home Page', () => {

  test('should load home page and display title', async ({ page }) => {
    // Arrange & Act - Navigate to homepage and wait for app to load
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);

    // Assert - Check for title
    await expect(page).toHaveTitle(/PoTicTac/i);
    const gameTitle = page.locator('.game-title, h1:has-text("POTICTAC")').first();
    await expect(gameTitle).toBeVisible({ timeout: 15000 });
    await expect(gameTitle).toContainText('POTICTAC');
  });

  test('should display game mode buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check for mode selection button (Single Player only in current implementation)
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
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
    
    // Need to select Single Player first to reveal Start Game button
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await singlePlayerButton.click();
    
    // Assert - Check for stats links (Leaderboard)
    const leaderboardButton = page.locator('button.stats-link').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardButton).toBeVisible({ timeout: 15000 });
  });

  test('should display game container after starting game', async ({ page }) => {
    // Arrange & Act - Start a single player game
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    const singlePlayerBtn = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerBtn).toBeVisible({ timeout: 10000 });
    await singlePlayerBtn.click();
    
    const startGameButton = page.locator('.start-game-button');
    await expect(startGameButton).toBeVisible({ timeout: 5000 });
    await startGameButton.click();
    
    // Assert - Check for game container in active game
    const gameContainer = page.locator('.game-container');
    await expect(gameContainer).toBeVisible({ timeout: 15000 });
  });

  test('should have no accessibility violations', async ({ page }) => {
    // Arrange
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    await page.waitForTimeout(500); // Wait for full page render
    
    // Act - Run accessibility scan on loaded page
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();
    
    // Assert
    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should be responsive on mobile viewport', async ({ page, viewport }) => {
    // Arrange & Act
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Verify page renders within viewport
    const body = page.locator('body');
    const boundingBox = await body.boundingBox();
    
    if (boundingBox && viewport) {
      expect(boundingBox.width).toBeLessThanOrEqual(viewport.width);
    }
  });
});