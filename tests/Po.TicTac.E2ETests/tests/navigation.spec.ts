import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Navigation Links', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
  });

  test('should display Player Stats and Leaderboard buttons on home page', async ({ page }) => {
    // Assert - Both navigation buttons should be visible (use flexible selectors)
    const playerStatsButton = page.locator('button.stats-button').filter({ hasText: 'Player Stats' });
    const leaderboardButton = page.locator('button.stats-button').filter({ hasText: 'Leaderboard' });
    
    await expect(playerStatsButton).toBeVisible({ timeout: 15000 });
    await expect(leaderboardButton).toBeVisible({ timeout: 15000 });
  });

  test('should navigate to Player Stats page from home', async ({ page }) => {
    // Arrange
    const playerStatsButton = page.locator('button.stats-button').filter({ hasText: 'Player Stats' });
    await expect(playerStatsButton).toBeVisible({ timeout: 15000 });
    
    // Act
    await playerStatsButton.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('playerstats');
    await expect(page.locator('.stats-title, h1:has-text("Player Statistics")').first()).toBeVisible({ timeout: 15000 });
  });

  test('should navigate to Leaderboard page from home', async ({ page }) => {
    // Arrange
    const leaderboardButton = page.locator('button.stats-button').filter({ hasText: 'Leaderboard' });
    await expect(leaderboardButton).toBeVisible({ timeout: 15000 });
    
    // Act
    await leaderboardButton.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('leaderboard');
    await expect(page.locator('.stats-title, h1:has-text("Leaderboard")').first()).toBeVisible({ timeout: 15000 });
  });

  test('should navigate back to home from Player Stats page', async ({ page }) => {
    // Arrange - Go to Player Stats page
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click back to home link
    const backToHomeLink = page.locator('a.stats-back-button').filter({ hasText: 'Back to Home' });
    await expect(backToHomeLink).toBeVisible({ timeout: 15000 });
    await backToHomeLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toMatch(/\/$/);
    await expect(page.locator('.game-title, h1:has-text("POTICTAC")').first()).toBeVisible({ timeout: 15000 });
  });

  test('should navigate back to home from Leaderboard page', async ({ page }) => {
    // Arrange - Go to Leaderboard page
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click back to home link
    const backToHomeLink = page.locator('a.stats-back-button').filter({ hasText: 'Back to Home' });
    await expect(backToHomeLink).toBeVisible({ timeout: 15000 });
    await backToHomeLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toMatch(/\/$/);
    await expect(page.locator('.game-title, h1:has-text("POTICTAC")').first()).toBeVisible({ timeout: 15000 });
  });

  test('should navigate from Player Stats to Leaderboard', async ({ page }) => {
    // Arrange - Go to Player Stats page
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click View Leaderboard link
    const leaderboardLink = page.locator('a.stats-back-button').filter({ hasText: 'View Leaderboard' });
    await expect(leaderboardLink).toBeVisible({ timeout: 15000 });
    await leaderboardLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('leaderboard');
    await expect(page.locator('.stats-title, h1:has-text("Leaderboard")').first()).toBeVisible({ timeout: 15000 });
  });

  test('should navigate from Leaderboard to Player Stats', async ({ page }) => {
    // Arrange - Go to Leaderboard page
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Click View Player Stats link
    const playerStatsLink = page.locator('a.stats-back-button').filter({ hasText: 'View Player Stats' });
    await expect(playerStatsLink).toBeVisible({ timeout: 15000 });
    await playerStatsLink.click();
    await waitForBlazorLoad(page);
    
    // Assert
    expect(page.url()).toContain('playerstats');
    await expect(page.locator('.stats-title, h1:has-text("Player Statistics")').first()).toBeVisible({ timeout: 15000 });
  });

  test('Player Stats page should load directly', async ({ page }) => {
    // Act
    await page.goto('/playerstats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check page content (SPA uses single title "PoTicTac")
    await expect(page.locator('.stats-container')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.stats-title, h1:has-text("Player Statistics")').first()).toBeVisible({ timeout: 15000 });
  });

  test('Leaderboard page should load directly', async ({ page }) => {
    // Act
    await page.goto('/leaderboard', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Assert - Check page content (SPA uses single title "PoTicTac")
    await expect(page.locator('.stats-container')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.stats-title, h1:has-text("Leaderboard")').first()).toBeVisible({ timeout: 15000 });
  });
});
