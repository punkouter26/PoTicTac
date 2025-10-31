import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('PoTicTac Home Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should load home page successfully', async ({ page }) => {
    // Arrange & Act - page already navigated in beforeEach
    
    // Assert
    await expect(page).toHaveTitle(/PoTicTac/i);
  });

  test('should display game board', async ({ page }) => {
    // Arrange & Act
    await page.waitForLoadState('networkidle');
    
    // Assert - Check for game board container
    const gameBoard = page.locator('[data-testid="game-board"], .game-board, board');
    await expect(gameBoard.first()).toBeVisible({ timeout: 10000 });
  });

  test('should have no accessibility violations', async ({ page }) => {
    // Arrange
    await page.waitForLoadState('networkidle');
    
    // Act
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();
    
    // Assert
    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should be responsive on mobile viewport', async ({ page, viewport }) => {
    // Arrange
    await page.waitForLoadState('networkidle');
    
    // Act & Assert - Verify page renders within viewport
    const body = page.locator('body');
    const boundingBox = await body.boundingBox();
    
    if (boundingBox && viewport) {
      expect(boundingBox.width).toBeLessThanOrEqual(viewport.width);
    }
  });
});
