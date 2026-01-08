import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Navigation Links', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
  });

  test('should display Player Stats and Leaderboard buttons on home page', async ({ page }) => {
    // Assert - Both navigation buttons should be visible
    const playerStatsButton = page.locator('button.stats-button:has-text("Player Stats")');
    const leaderboardButton = page.locator('button.stats-button:has-text("Leaderboard")');
    
    await expect(playerStatsButton).toBeVisible({ timeout: 10000 });
    await expect(leaderboardButton).toBeVisible({ timeout: 10000 });
  });

  test('should navigate to Player Stats page from home', async ({ page }) => {
    // Arrange
    const playerStatsButton = page.locator('button.stats-button:has-text("Player Stats")');
    
    // Act
    await playerStatsButton.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('playerstats');
    await expect(page.locator('h1.stats-title')).toContainText('Player Statistics');
  });

  test('should navigate to Leaderboard page from home', async ({ page }) => {
    // Arrange
    const leaderboardButton = page.locator('button.stats-button:has-text("Leaderboard")');
    
    // Act
    await leaderboardButton.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('leaderboard');
    await expect(page.locator('h1.stats-title')).toContainText('Leaderboard');
  });

  test('should navigate back to home from Player Stats page', async ({ page }) => {
    // Arrange - Go to Player Stats page
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click back to home link
    const backToHomeLink = page.locator('a.stats-back-button:has-text("Back to Home")');
    await backToHomeLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toMatch(/\/$/);
    await expect(page.locator('h1.game-title')).toContainText('POTICTAC');
  });

  test('should navigate back to home from Leaderboard page', async ({ page }) => {
    // Arrange - Go to Leaderboard page
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click back to home link
    const backToHomeLink = page.locator('a.stats-back-button:has-text("Back to Home")');
    await backToHomeLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toMatch(/\/$/);
    await expect(page.locator('h1.game-title')).toContainText('POTICTAC');
  });

  test('should navigate from Player Stats to Leaderboard', async ({ page }) => {
    // Arrange - Go to Player Stats page
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click View Leaderboard link
    const leaderboardLink = page.locator('a.stats-back-button:has-text("View Leaderboard")');
    await leaderboardLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('leaderboard');
    await expect(page.locator('h1.stats-title')).toContainText('Leaderboard');
  });

  test('should navigate from Leaderboard to Player Stats', async ({ page }) => {
    // Arrange - Go to Leaderboard page
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click View Player Stats link
    const playerStatsLink = page.locator('a.stats-back-button:has-text("View Player Stats")');
    await playerStatsLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('playerstats');
    await expect(page.locator('h1.stats-title')).toContainText('Player Statistics');
  });

  test('Player Stats page should load directly', async ({ page }) => {
    // Act
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check page content (SPA uses single title "PoTicTac")
    await expect(page.locator('.stats-container')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('h1.stats-title')).toContainText('Player Statistics');
  });

  test('Leaderboard page should load directly', async ({ page }) => {
    // Act
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check page content (SPA uses single title "PoTicTac")
    await expect(page.locator('.stats-container')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('h1.stats-title')).toContainText('Leaderboard');
  });
});
