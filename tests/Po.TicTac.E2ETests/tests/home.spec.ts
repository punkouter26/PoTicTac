import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { waitForBlazorLoad, waitForElement } from './helpers';

test.describe('PoTicTac Home Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
  });

  test('should load home page successfully', async ({ page }) => {
    // Assert - Check title
    await expect(page).toHaveTitle(/PoTicTac/i);
    
    // Verify main title is visible - use more flexible selector
    const gameTitle = page.locator('.game-title, h1:has-text("POTICTAC")').first();
    await expect(gameTitle).toBeVisible({ timeout: 15000 });
  });

  test('should display game board', async ({ page }) => {
    // Arrange & Act - Start a single player game first
    const singlePlayerBtn = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerBtn).toBeVisible({ timeout: 10000 });
    await singlePlayerBtn.click();
    
    // Assert - Check for game board container in active game
    const gameBoard = page.locator('.game-container');
    await expect(gameBoard).toBeVisible({ timeout: 15000 });
  });

  test('should have no accessibility violations', async ({ page }) => {
    // Wait for full page render
    await page.waitForTimeout(500);
    
    // Act - Run accessibility scan on loaded page
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();
    
    // Assert
    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should be responsive on mobile viewport', async ({ page, viewport }) => {
    // Act & Assert - Verify page renders within viewport
    const body = page.locator('body');
    const boundingBox = await body.boundingBox();
    
    if (boundingBox && viewport) {
      expect(boundingBox.width).toBeLessThanOrEqual(viewport.width);
    }
  });
});
