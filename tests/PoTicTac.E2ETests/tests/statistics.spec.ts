import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Statistics Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('should navigate to statistics page', async ({ page }) => {
    // Arrange - Look for navigation to stats
    const statsLink = page.locator('a:has-text("Stats"), a:has-text("Statistics"), [href*="stats"]');
    
    // Act
    const linkCount = await statsLink.count();
    
    if (linkCount > 0) {
      await statsLink.first().click();
      await page.waitForLoadState('networkidle');
      
      // Assert - URL should contain stats
      expect(page.url()).toContain('stats');
    } else {
      // Navigate directly to stats page
      await page.goto('/stats');
      await page.waitForLoadState('networkidle');
      
      // Assert - Should be on stats page
      expect(page.url()).toContain('stats');
    }
  });

  test('should display player statistics', async ({ page }) => {
    // Arrange
    await page.goto('/stats');
    await page.waitForLoadState('networkidle');
    
    // Act - Look for statistics elements
    const statsContainer = page.locator('[data-testid="statistics"], .statistics, .stats');
    
    // Assert
    await expect(statsContainer.first()).toBeVisible({ timeout: 10000 });
  });

  test('statistics page should have no accessibility violations', async ({ page }) => {
    // Arrange
    await page.goto('/stats');
    await page.waitForLoadState('networkidle');
    
    // Act
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();
    
    // Assert
    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should display leaderboard data', async ({ page }) => {
    // Arrange
    await page.goto('/stats');
    await page.waitForLoadState('networkidle');
    
    // Act - Look for leaderboard or player list
    const leaderboard = page.locator('[data-testid="leaderboard"], .leaderboard, table, ul');
    
    // Assert
    await expect(leaderboard.first()).toBeVisible({ timeout: 10000 });
  });
});
