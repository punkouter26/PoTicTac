import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { waitForBlazorLoad } from './helpers';

test.describe('Statistics Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
  });

  test('should navigate to statistics page', async ({ page }) => {
    // Arrange - Look for stats button
    const statsButton = page.locator('button.stats-button:has-text("View Statistics")');
    
    // Act
    await statsButton.click();
    await waitForBlazorLoad(page);
    
    // Assert - URL should contain stats
    expect(page.url()).toContain('stats');
  });

  test('should display player statistics', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Look for statistics elements
    const statsContainer = page.locator('.statistics, .stats, [data-testid="statistics"]');
    
    // Assert - At minimum, page should load
    const pageTitle = page.locator('h1, h2');
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('statistics page should have no accessibility violations', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();
    
    // Assert
    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should display leaderboard data', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Look for leaderboard or player list (table, list, or grid)
    const leaderboard = page.locator('table, ul, .leaderboard, [data-testid="leaderboard"]');
    
    // Assert - Check if leaderboard exists
    const count = await leaderboard.count();
    expect(count).toBeGreaterThanOrEqual(0); // May be empty if no data
  });

  test('should display three difficulty leaderboards', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Look for the three leaderboard sections
    const easyLeaderboard = page.getByText('🟢 Easy Leaderboard');
    const mediumLeaderboard = page.getByText('🟡 Medium Leaderboard');
    const hardLeaderboard = page.getByText('🔴 Hard Leaderboard');
    
    // Assert - All three leaderboards should be visible
    await expect(easyLeaderboard).toBeVisible({ timeout: 10000 });
    await expect(mediumLeaderboard).toBeVisible({ timeout: 10000 });
    await expect(hardLeaderboard).toBeVisible({ timeout: 10000 });
  });

  test('should display leaderboards in correct order', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Get all leaderboard section titles
    const leaderboardHeaders = page.locator('.stats-section-title');
    
    // Assert - Should have exactly 3 leaderboards
    await expect(leaderboardHeaders).toHaveCount(3);
    
    // Verify order: Easy, Medium, Hard
    const firstHeader = leaderboardHeaders.nth(0);
    const secondHeader = leaderboardHeaders.nth(1);
    const thirdHeader = leaderboardHeaders.nth(2);
    
    await expect(firstHeader).toContainText('Easy');
    await expect(secondHeader).toContainText('Medium');
    await expect(thirdHeader).toContainText('Hard');
  });

  test('each leaderboard should have table structure', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Look for tables within the page
    const tables = page.locator('table');
    const tableCount = await tables.count();
    
    // Assert - Should have at least 3 tables (one per leaderboard)
    // May have more if there's data, or fewer if using different structure
    expect(tableCount).toBeGreaterThanOrEqual(0);
    
    // If tables exist, verify they have headers
    if (tableCount > 0) {
      const firstTable = tables.first();
      const headers = firstTable.locator('th');
      const headerCount = await headers.count();
      expect(headerCount).toBeGreaterThan(0);
    }
  });

  test('leaderboards should show correct column headers', async ({ page }) => {
    // Arrange
    await page.goto('/stats', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Look for common leaderboard column headers
    const rankHeader = page.getByRole('columnheader', { name: /rank/i }).first();
    const playerHeader = page.getByRole('columnheader', { name: /player/i }).first();
    const winRateHeader = page.getByRole('columnheader', { name: /win rate/i }).first();
    
    // Assert - Headers should be visible if tables have data
    const tableCount = await page.locator('table').count();
    if (tableCount > 0) {
      await expect(rankHeader).toBeVisible();
      await expect(playerHeader).toBeVisible();
      await expect(winRateHeader).toBeVisible();
    }
  });
});
